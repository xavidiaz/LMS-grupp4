using LMS.Blazor.Client.Helper;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace LMS.Blazor.Client.Services.ApiProxy;

public sealed class ApiProxyClient(HttpClient httpClient)  : IApiProxyClient 
{
    public async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        var proxyUri = GetProxyUri(endpoint);
        var errorMessage = "";
        using var request = new HttpRequestMessage(method, proxyUri)
        {
            Content = content
        };

        if (RequiresAntiforgery(method))
        {
            var requestToken =
                await GetAntiforgeryTokenAsync(cancellationToken);

            request.Headers.Add(
                AntiforgeryProtocol.HeaderName,
                requestToken);
        }

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        // Read the API error code before throwing.
        if (!response.IsSuccessStatusCode)
        {
            errorMessage= response.Content != null
                ? await response.Content.ReadAsStringAsync(cancellationToken)
                : "The API returned an error without a message.";
            var error = JsonSerializer.Deserialize<ApiError>(errorMessage);

            if (response.StatusCode == HttpStatusCode.Unauthorized) { throw new UnauthorizedAccessException("The Blazor session is no longer valid."); }

            throw new ApiException(response.StatusCode, error.ToString());
        }

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content
            .ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    private async Task<string> GetAntiforgeryTokenAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            AntiforgeryProtocol.TokenEndpoint,
            cancellationToken);

        EnsureAuthenticated(response);
        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<AntiforgeryTokenDto>(
                cancellationToken);

        return !string.IsNullOrWhiteSpace(token?.RequestToken)
            ? token.RequestToken
            : throw new InvalidOperationException(
                "The BFF returned an invalid antiforgery token response.");
    }

    private static void EnsureAuthenticated(
        HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "The Blazor session is no longer valid.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                "You do not have permission to access this resource.",
                null,
                HttpStatusCode.Forbidden);
        }
    }

    private static bool RequiresAntiforgery(HttpMethod method) =>
        method != HttpMethod.Get &&
        method != HttpMethod.Head &&
        method != HttpMethod.Options &&
        method != HttpMethod.Trace;

    private static string GetProxyUri(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint) ||
            endpoint.StartsWith('/') ||
            endpoint.StartsWith('\\') ||
            endpoint.Contains('\\') ||
            endpoint.Contains('#') ||
            Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            throw new ArgumentException(
                "A non-empty relative API path is required.",
                nameof(endpoint));
        }

        var path = endpoint.Split('?', 2)[0];

        if (Uri.UnescapeDataString(path)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException(
                "Relative API paths cannot contain traversal segments.",
                nameof(endpoint));
        }

        return $"api/proxy/{endpoint}";
    }
}