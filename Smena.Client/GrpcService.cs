using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Http;

namespace Smena.Client;

public class GrpcService
{
    public GrpcChannel Channel { get; }
    public CallInvoker CallInvoker { get; }
    private readonly HttpClient _httpClient;

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
}
