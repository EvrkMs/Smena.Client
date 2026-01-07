using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services.Advance;

namespace Smena.Client.Services;

public class AdvanceService
{
    private readonly GrpcAdvanceService.GrpcAdvanceServiceClient _client;

    public AdvanceService(GrpcChannel channel)
    {
        _client = new(channel);
    }

    public async Task<(bool Success, string Message)> SendAdvanceAsync(GrpcAdvanceRequest request)
    {
        try
        {
            var response = await _client.SendAdvanceAsync(request);
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