using System.Net.Sockets;
using Gml.Core.Helpers.Servers;

namespace GmlCore.Tests;

public class ServersPingTest
{
    [Test]
    [Order(41)]
    public async Task ServerPing1_20_6_WithDomain()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.20.6
            var status = await JavaServerPinger.PingAsync("mc.hushcraft.ru", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(42)]
    public async Task ServerPing1_7_10()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.7.10
            var status = await JavaServerPinger.PingAsync("79.133.181.47", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(43)]
    public async Task ServerPing1_5_2()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.5.2
            var status = await JavaServerPinger.PingAsync("146.19.48.159", 25777, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(44)]
    public async Task ServerPing1_12_2()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.12.2
            var status = await JavaServerPinger.PingAsync("185.9.145.192", 20679, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(45)]
    public async Task ServerPing1_16_5()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.16.5
            var status = await JavaServerPinger.PingAsync("144.76.61.107", 20690, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_20_1()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.20.1
            var status = await JavaServerPinger.PingAsync("199.83.103.196", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21
            var status = await JavaServerPinger.PingAsync("213.152.43.2", 25616, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21_1()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21.1
            var status = await JavaServerPinger.PingAsync("MCR7.GGMINE.RU", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21_2()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21.2
            var status = await JavaServerPinger.PingAsync("ilp.tridentmc.ru", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21_3()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21.3
            var status = await JavaServerPinger.PingAsync("130.61.138.213", 25565, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21_4()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21.4
            var status = await JavaServerPinger.PingAsync("185.9.145.101", 25748, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }

    [Test]
    [Order(46)]
    public async Task ServerPing1_21_5()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);

            // 1.21.5
            var status = await JavaServerPinger.PingAsync("135.181.237.44", 25750, TimeSpan.FromMilliseconds(300),
                cancellationTokenSource.Token);

            Console.WriteLine($"{status.OnlinePlayers} / {status.MaximumPlayers}");

            Assert.That(status, Is.Not.Null);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Server not allowed: {e.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ping operation was cancelled");
        }
    }
}
