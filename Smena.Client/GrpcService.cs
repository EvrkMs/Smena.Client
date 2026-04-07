using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smena.Client;

/// <summary>Prepends a fixed path prefix to every gRPC request URI.</summary>
internal sealed class PathPrefixHandler(HttpMessageHandler inner, string prefix) : DelegatingHandler(inner)
{
    private readonly string _prefix = prefix.TrimEnd('/');

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.RequestUri is { } uri)
        {
            var b = new UriBuilder(uri);
            b.Path = _prefix + (b.Path.StartsWith('/') ? b.Path : '/' + b.Path);
            request.RequestUri = b.Uri;
        }
        return base.SendAsync(request, ct);
    }
}

public class GrpcService : IDisposable, IAsyncDisposable
{
    public GrpcChannel Channel { get; }
    public CallInvoker CallInvoker { get; }
    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <param name="address">Base address of the server, e.g. https://example.com</param>
    /// <param name="apiKey">API key sent in x-api-key header.</param>
    /// <param name="pathPrefix">Optional path prefix for gRPC, e.g. /grpc.</param>
    public GrpcService(string address, string apiKey, string? pathPrefix = null)
    {
        var innerHandler = new HttpClientHandler();
        HttpMessageHandler handler = string.IsNullOrWhiteSpace(pathPrefix)
            ? innerHandler
            : new PathPrefixHandler(innerHandler, pathPrefix);

        _httpClient = new HttpClient(handler)
        {
            // Closing shift may include photo and Telegram operations that exceed the default 100s timeout.
            Timeout = ShiftConstants.GrpcHttpTimeout
        };
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        Channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpClient = _httpClient
        });
        CallInvoker = Channel.CreateCallInvoker();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Channel.Dispose();
        _httpClient.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await Channel.ShutdownAsync();
        Channel.Dispose();
        _httpClient.Dispose();
    }
}
