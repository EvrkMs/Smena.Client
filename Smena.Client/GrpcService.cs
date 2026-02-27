using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Http;

namespace Smena.Client;

public class GrpcService : IDisposable, IAsyncDisposable
{
    public GrpcChannel Channel { get; }
    public CallInvoker CallInvoker { get; }
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public GrpcService(string address, string apiKey)
    {
        _httpClient = new HttpClient();
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

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
