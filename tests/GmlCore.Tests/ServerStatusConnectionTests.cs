using System.Net;
using System.Net.Sockets;
using Gml.Core.Helpers.Profiles;
using Gml.Core.Helpers.Servers;
using Gml.Models.Servers;

namespace GmlCore.Tests;

/// <summary>
///     The launcher API pings every profile server every 2 minutes. These tests pin down that each ping
///     closes its connection: Pingo left the socket open, and the game server logged a read timeout for
///     every leaked connection.
/// </summary>
public class ServerStatusConnectionTests
{
    private static readonly TimeSpan CloseWait = TimeSpan.FromSeconds(3);

    [Test]
    public async Task UpdateServerState_ReadsStatusAndClosesConnection()
    {
        using FakeMinecraftStatusServer fake = new();
        // UpdateServerState uses none of the procedures' dependencies.
        ProfileProcedures procedures = new(null!, null!, null!, null!, null!);
        MinecraftServer server = new() { Name = "Waffle", Address = "127.0.0.1", Port = fake.Port };

        await procedures.UpdateServerState(server);

        Assert.Multiple(() =>
        {
            Assert.That(server.IsOnline, Is.True);
            Assert.That(server.Online, Is.EqualTo(3));
            Assert.That(server.MaxOnline, Is.EqualTo(20));
            Assert.That(server.Version, Is.EqualTo("1.20.1"));
        });
        Assert.That(await fake.WaitForClientCloseAsync(CloseWait), Is.True,
            "the ping must close its connection instead of leaving it to the server's read timeout");
    }

    [Test]
    public async Task UpdateServerState_MarksServerOfflineWhenNothingListens()
    {
        int port = FreePort();
        ProfileProcedures procedures = new(null!, null!, null!, null!, null!);
        MinecraftServer server = new()
        {
            Name = "Waffle", Address = "127.0.0.1", Port = port, IsOnline = true, Online = 5, MaxOnline = 20
        };

        await procedures.UpdateServerState(server);

        Assert.Multiple(() =>
        {
            Assert.That(server.IsOnline, Is.False);
            Assert.That(server.Online, Is.Null);
            Assert.That(server.MaxOnline, Is.Null);
        });
    }

    [Test]
    public void PingAsync_TimesOutAndClosesConnectionWhenServerStaysSilent()
    {
        using FakeMinecraftStatusServer fake = new(respond: false);

        Assert.ThrowsAsync(Is.InstanceOf<OperationCanceledException>(), () =>
            JavaServerPinger.PingAsync("127.0.0.1", fake.Port, TimeSpan.FromMilliseconds(300)));
        Assert.That(fake.WaitForClientCloseAsync(CloseWait).Result, Is.True);
        Assert.That(fake.FramesReceived, Is.EqualTo(2), "handshake and status request are sent before waiting");
    }

    [Test]
    public async Task PingAsync_ParsesStatusResponse()
    {
        using FakeMinecraftStatusServer fake = new();

        JavaServerStatus status = await JavaServerPinger.PingAsync("127.0.0.1", fake.Port, TimeSpan.FromSeconds(5));

        Assert.That(status, Is.EqualTo(new JavaServerStatus("1.20.1", 3, 20)));
    }

    private static int FreePort()
    {
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
