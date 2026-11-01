using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gml.Core.Helpers.Servers;

public sealed record JavaServerStatus(string VersionName, int OnlinePlayers, int MaximumPlayers);

/// <summary>
///     Minecraft Java Edition server list ping (handshake + status request). The connection is always closed
///     when the call returns: a socket left open makes the game server drop it on its read timeout and log
///     an error for every ping.
/// </summary>
public static class JavaServerPinger
{
    private const int MaxFrameLength = 1 << 20;

    /// <exception cref="OperationCanceledException">The server did not answer within <paramref name="timeout" />.</exception>
    /// <exception cref="SocketException">The server could not be reached.</exception>
    /// <exception cref="InvalidDataException">The server sent something other than a status response.</exception>
    public static async Task<JavaServerStatus> PingAsync(string address, int port, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        CancellationToken token = timeoutSource.Token;

        using TcpClient client = new();
        await client.ConnectAsync(address, port, token);
        NetworkStream stream = client.GetStream();

        await stream.WriteAsync(Frame(Handshake(address, port)), token);
        await stream.WriteAsync(Frame([0x00]), token);

        byte[] payload = await ReadFrameAsync(stream, token);
        return ParseStatusResponse(payload);
    }

    private static byte[] Handshake(string address, int port)
    {
        List<byte> packet = [0x00];
        WriteVarInt(packet, -1); // protocol version: any, status only
        byte[] host = Encoding.UTF8.GetBytes(address);
        WriteVarInt(packet, host.Length);
        packet.AddRange(host);
        Span<byte> portBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(portBytes, (ushort)port);
        packet.AddRange(portBytes.ToArray());
        WriteVarInt(packet, 1); // next state: status
        return packet.ToArray();
    }

    private static byte[] Frame(byte[] packet)
    {
        List<byte> frame = new(packet.Length + 5);
        WriteVarInt(frame, packet.Length);
        frame.AddRange(packet);
        return frame.ToArray();
    }

    private static void WriteVarInt(List<byte> buffer, int value)
    {
        uint remaining = (uint)value;
        do
        {
            byte current = (byte)(remaining & 0x7F);
            remaining >>= 7;
            if (remaining != 0) current |= 0x80;
            buffer.Add(current);
        } while (remaining != 0);
    }

    private static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken token)
    {
        int length = 0;
        byte[] one = new byte[1];
        for (int shift = 0;; shift += 7)
        {
            if (shift >= 35) throw new InvalidDataException("Frame length VarInt is too long.");
            await stream.ReadExactlyAsync(one, token);
            length |= (one[0] & 0x7F) << shift;
            if ((one[0] & 0x80) == 0) break;
        }

        if (length is <= 0 or > MaxFrameLength)
            throw new InvalidDataException($"Unexpected status frame length {length}.");

        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload, token);
        return payload;
    }

    private static JavaServerStatus ParseStatusResponse(byte[] payload)
    {
        int offset = 0;
        int packetId = ReadVarInt(payload, ref offset);
        if (packetId != 0x00) throw new InvalidDataException($"Unexpected packet 0x{packetId:X2} instead of status.");

        int jsonLength = ReadVarInt(payload, ref offset);
        if (jsonLength < 0 || offset + jsonLength > payload.Length)
            throw new InvalidDataException("Status JSON length is out of range.");

        using JsonDocument document = JsonDocument.Parse(payload.AsMemory(offset, jsonLength));
        JsonElement root = document.RootElement;

        string versionName = root.TryGetProperty("version", out JsonElement version)
                             && version.TryGetProperty("name", out JsonElement name)
                             && name.ValueKind == JsonValueKind.String
            ? name.GetString()!
            : string.Empty;

        int online = 0, maximum = 0;
        if (root.TryGetProperty("players", out JsonElement players))
        {
            if (players.TryGetProperty("online", out JsonElement onlineElement)) online = onlineElement.GetInt32();
            if (players.TryGetProperty("max", out JsonElement maxElement)) maximum = maxElement.GetInt32();
        }

        return new JavaServerStatus(versionName, online, maximum);
    }

    private static int ReadVarInt(byte[] buffer, ref int offset)
    {
        int value = 0;
        for (int shift = 0; shift < 35; shift += 7)
        {
            if (offset >= buffer.Length) throw new InvalidDataException("Truncated VarInt.");
            byte current = buffer[offset++];
            value |= (current & 0x7F) << shift;
            if ((current & 0x80) == 0) return value;
        }

        throw new InvalidDataException("VarInt is too long.");
    }
}
