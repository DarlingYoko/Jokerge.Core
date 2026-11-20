using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gml.Common.TextureService;
using GmlCore.Interfaces.Integrations;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.User;
using Newtonsoft.Json;

namespace Gml.Core.Integrations;

public class TextureProvider(
    string textureServiceEndpoint,
    IBugTrackerProcedures bugTracker,
    HttpMessageHandler? handler = null) : ITextureProvider
{
    private readonly string _cloakPrefix = "-c";

    private readonly HttpClient _httpClientLoader = CreateClient(handler, new Uri(textureServiceEndpoint));

    private readonly HttpClient _httpClintSkinChecker = CreateClient(handler, null);

    private readonly string _skinPrefix = "-s";

    public async Task<string> SetSkin(IUser user, string skinUrl)
    {
        try
        {
            var requestUri = $"skin/{user.Name}";

            var model = await UpdateTexture(user, skinUrl, requestUri, _skinPrefix);

            return model?.SkinUrl!;
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            Debug.WriteLine(exception);
            return string.Empty;
        }
        catch (Exception exception)
        {
            bugTracker.CaptureException(exception);
            Debug.WriteLine(exception);
            return string.Empty;
        }
    }

    public async Task<string> SetCloak(IUser user, string cloakUrl)
    {
        try
        {
            var requestUri = $"cloak/{user.Name}";

            var model = await UpdateTexture(user, cloakUrl, requestUri, _cloakPrefix);

            return model?.ClockUrl!;
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            Debug.WriteLine(exception);
            return string.Empty;
        }
        catch (Exception exception)
        {
            bugTracker.CaptureException(exception);
            Debug.WriteLine(exception);
            return string.Empty;
        }
    }

    public Task DeleteSkin(string userName)
    {
        return DeleteTexture($"skin/{userName}");
    }

    public Task DeleteCloak(string userName)
    {
        return DeleteTexture($"cloak/{userName}");
    }

    private async Task DeleteTexture(string requestUri)
    {
        try
        {
            var response = await _httpClientLoader.DeleteAsync(requestUri);

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
                response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            bugTracker.CaptureException(exception);
            Debug.WriteLine(exception);
        }
    }

    public Task<Stream> GetSkinStream(string? textureUrl)
    {
        return _httpClintSkinChecker.GetStreamAsync(textureUrl);
    }

    public Task<Stream> GetCloakStream(string? textureUrl)
    {
        return _httpClintSkinChecker.GetStreamAsync(textureUrl);
    }

    public Task<Stream> GetHeadByNameStream(string? userName)
    {
        return _httpClientLoader.GetStreamAsync($"/skin/{userName}/head/128");
    }

    public async Task<(string? SkinUrl, bool HasSkin, string? CloakUrl, bool HasCloak)> GetUserTexture(string userName)
    {
        try
        {
            using var response = await _httpClientLoader.GetAsync($"/{userName}");

            // The service rejects names it cannot store (only A-Z, 0-9, _ and -): no texture, not a bug.
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
                return (null, false, null, false);

            var json = await ReadSuccessAsync(response, $"GET /{userName}");
            var model = JsonConvert.DeserializeObject<TextureReadDto>(json);

            return (model?.SkinUrl, model?.HasSkin ?? false, model?.ClockUrl, model?.HasCloak ?? false);
        }
        catch (Exception exception)
        {
            bugTracker.CaptureException(exception);
            Debug.WriteLine(exception);
            return (null, false, null, false);
        }
    }

    private async Task<TextureReadDto?> UpdateTexture(IUser user, string skinUrl, string requestUri, string prefix)
    {
        var skinResponseMessage = await _httpClintSkinChecker.GetStreamAsync(skinUrl);

        if (skinResponseMessage is not null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var content = new MultipartFormDataContent();

            //ToDo: GetFileSize
            content.Add(new StreamContent(skinResponseMessage, 1000), "file", $"{prefix}{user.Name}.png");

            request.Content = content;
            using var response = await _httpClientLoader.SendAsync(request);

            var data = await ReadSuccessAsync(response, $"POST {requestUri}");

            return JsonConvert.DeserializeObject<TextureReadDto>(data);
        }

        return null;
    }

    private static HttpClient CreateClient(HttpMessageHandler? handler, Uri? baseAddress)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        client.BaseAddress = baseAddress;
        return client;
    }

    /// <summary>
    ///     The service answers errors with a plain-text reason ("Error saving cloak: ..."). Raise it with
    ///     the status so the bug report says what failed, instead of a JSON conversion error or a bare
    ///     status line.
    /// </summary>
    private static async Task<string> ReadSuccessAsync(HttpResponseMessage response, string operation)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return body;

        var reason = body.Trim().Trim('"');
        if (reason.Length > 500)
            reason = reason[..500];

        throw new HttpRequestException(
            $"Texture service {operation} returned {(int)response.StatusCode} ({response.StatusCode}): {reason}",
            null, response.StatusCode);
    }
}
