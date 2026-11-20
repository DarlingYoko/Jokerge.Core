using System.Net;
using System.Reflection;
using Gml.Core.Integrations;
using Gml.Models.User;
using GmlCore.Interfaces.Procedures;

namespace GmlCore.Tests;

/// <summary>
///     When the skin service answered with an error, the panel's bug list showed either a bare
///     "500 (Internal Server Error)" or a JSON conversion failure of the service's error text,
///     instead of what actually went wrong.
/// </summary>
public class TextureProviderTests
{
    private const string Endpoint = "http://skins.test";

    private static (TextureProvider Provider, List<Exception> Captured) Create(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var tracker = CapturingTracker.Create(out var captured);
        return (new TextureProvider(Endpoint, tracker, new StubHandler(respond)), captured);
    }

    private static HttpResponseMessage Reply(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body) };

    [Test]
    public async Task SetCloak_ServiceRejectsUpload_ReportsItsMessageNotAJsonError()
    {
        var (provider, captured) = Create(request => request.Method == HttpMethod.Get
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) }
            : Reply(HttpStatusCode.BadRequest, "\"Error saving cloak: file is busy\""));

        var url = await provider.SetCloak(new AuthUser { Name = "Darling_Yoko" }, "http://files.test/cloak.png");

        Assert.Multiple(() =>
        {
            Assert.That(url, Is.Empty);
            Assert.That(captured, Has.Count.EqualTo(1));
            Assert.That(captured[0], Is.TypeOf<HttpRequestException>());
            Assert.That(captured[0].Message, Does.Contain("400").And.Contain("Error saving cloak: file is busy"));
        });
    }

    [Test]
    public async Task GetUserTexture_ServiceFails_ReportsStatusAndBody()
    {
        var (provider, captured) = Create(_ => Reply(HttpStatusCode.InternalServerError, "boom"));

        var texture = await provider.GetUserTexture("Darling_Yoko");

        Assert.Multiple(() =>
        {
            Assert.That(texture.HasSkin, Is.False);
            Assert.That(captured, Has.Count.EqualTo(1));
            Assert.That(captured[0].Message, Does.Contain("500").And.Contain("boom").And.Contain("Darling_Yoko"));
        });
    }

    [Test]
    public async Task GetUserTexture_NameTheServiceRejects_ReturnsNoTextureWithoutReporting()
    {
        var (provider, captured) = Create(_ => Reply(HttpStatusCode.BadRequest, "Invalid username."));

        var texture = await provider.GetUserTexture("Игрок");

        Assert.Multiple(() =>
        {
            Assert.That(texture, Is.EqualTo(((string?)null, false, (string?)null, false)));
            Assert.That(captured, Is.Empty);
        });
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }

    public class CapturingTracker : DispatchProxy
    {
        private List<Exception> _captured = null!;

        public static IBugTrackerProcedures Create(out List<Exception> captured)
        {
            var proxy = Create<IBugTrackerProcedures, CapturingTracker>();
            ((CapturingTracker)(object)proxy)._captured = captured = [];
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == nameof(IBugTrackerProcedures.CaptureException) && args![0] is Exception e)
            {
                _captured.Add(e);
                return null;
            }

            throw new NotSupportedException(targetMethod.Name);
        }
    }
}
