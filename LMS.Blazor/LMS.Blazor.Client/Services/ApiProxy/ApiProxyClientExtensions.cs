using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services.ApiProxy;

public static class ApiProxyClientExtensions
{
    public static Task<TResponse?> GetAsync<TResponse>(
        this IApiProxyClient client,
        string endpoint,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Get,
            endpoint,
            cancellationToken: cancellationToken);
    public static Task<TResponse?> PostAsync<TRequest, TResponse>(
        this IApiProxyClient client,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Post,
            endpoint,
            JsonContent.Create(request),
            cancellationToken);


    public static Task<TResponse?> PutAsync<TRequest, TResponse>(
        this IApiProxyClient client,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Put,
            endpoint,
            JsonContent.Create(request),
            cancellationToken);


    public static Task<TResponse?> PatchAsync<TRequest, TResponse>(
        this IApiProxyClient client,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Patch,
            endpoint,
            JsonContent.Create(request),
            cancellationToken);


    public static async Task DeleteAsync(
        this IApiProxyClient client,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        await client.SendAsync<object?>(
            HttpMethod.Delete,
            endpoint,
            cancellationToken: cancellationToken);
    }
}