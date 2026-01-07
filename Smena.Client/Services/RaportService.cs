using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services.Raport;

namespace Smena.Client.Services;

public class RaportService(GrpcChannel channel)
{
    private readonly GrpcRaportService.GrpcRaportServiceClient _client = new(channel);

    public async Task<(bool Success, string Message)> SendRaportAsync(GrpcRaportRequest request)
    {
        try
        {
            var response = await _client.SendRaportAsync(request);
            return (response.Value, response.Message);
        }
        catch (RpcException ex)
        {
            return (false, $"gRPC ошибка: {ex.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }
}