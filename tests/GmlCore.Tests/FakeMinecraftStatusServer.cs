using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GmlCore.Tests;

/// <summary>
///     A one-connection Minecraft status server on loopback. It reads the handshake and the status request,
///     answers with <see cref="StatusJson" /> (unless <see cref="Respond" /> is false) and then records
///     whether the client closed its side of the connection.
/// </summary>
public sealed class FakeMinecraftStatusServer : IDisposable
{
    public const string StatusJson =
        "{\"version\":{\"name\":\"1.20.1\",\"protocol\":763},\"players\":{\"max\":20,\"online\":3}," +
        "\"description\":{\"text\":\"Waffle\"}}";

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly TaskCompletionSource<bool> _clientClosed =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public FakeMinecraftStatusServer(bool respond = true)
    {
        Respond = respond;
        _listener.Start();
        _ = ServeAsync();
    }

    public bool Respond { get; }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    /// <summary>Number of frames the client sent before it closed the connection.</summary>
    public int FramesReceived { get; private set; }

    public void Dispose()
    {
        _listener.Stop();
    }

    /// <summary>
    ///     True once the client sends FIN or resets the connection, false if it still holds the connection
    ///     open after <paramref name="timeout" />.
    /// </summary>
    public async Task<bool> WaitForClientCloseAsync(TimeSpan timeout)
    {
        Task finished = await Task.WhenAny(_clientClosed.Task, Task.Delay(timeout));
        return finished == _clientClosed.Task;
    }

    private async Task ServeAsync()
    {
        using TcpClient client = await _listener.AcceptTcpClientAsync();
        NetworkStream stream = client.GetStream();
        try
        {
            // Handshake, then status request.
            for (int i = 0; i < 2; i++)
            {
                if (!await TryReadFrameAsync(stream)) break;
                FramesReceived++;
            }

            if (Respond && FramesReceived == 2) await WriteStatusResponseAsync(stream);

            byte[] buffer = new byte[256];
            while (await stream.ReadAsync(buffer) > 0)
            {
            }
        }
        catch (IOException)
        {
            // A reset closes the connection too.
        }

        _clientClosed.TrySetResult(true);
    }

    private static async Task<bool> TryReadFrameAsync(NetworkStream stream)
    {
        int? length = await TryReadVarIntAsync(stream);
        if (length is null) return false;

        byte[] payload = new byte[length.Value];
        int read = 0;
        while (read < payload.Length)
        {
            int n = await stream.ReadAsync(payload.AsMemory(read));
            if (n == 0) return false;
            read += n;
        }

        return true;
    }

    private static async Task<int?> TryReadVarIntAsync(NetworkStream stream)
    {
        int value = 0;
        byte[] one = new byte[1];
        for (int shift = 0; shift < 35; shift += 7)
        {
            if (await stream.ReadAsync(one) == 0) return null;
            value |= (one[0] & 0x7F) << shift;
            if ((one[0] & 0x80) == 0) return value;
        }

        throw new InvalidDataException("VarInt is too long.");
    }

    private static async Task WriteStatusResponseAsync(NetworkStream stream)
    {
        byte[] json = Encoding.UTF8.GetBytes(StatusJson);
        List<byte> body = new() { 0x00 };
        body.AddRange(VarInt(json.Length));
        body.AddRange(json);

        List<byte> frame = new(VarInt(body.Count));
        frame.AddRange(body);
        await stream.WriteAsync(frame.ToArray());
    }

    private static IEnumerable<byte> VarInt(int value)
    {
        uint v = (uint)value;
        do
        {
            byte b = (byte)(v & 0x7F);
            v >>= 7;
            if (v != 0) b |= 0x80;
            yield return b;
        } while (v != 0);
    }
}
