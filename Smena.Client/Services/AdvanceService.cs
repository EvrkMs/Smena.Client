using Grpc.Core;
using Host.Grpc.Services.Advance;

namespace Smena.Client.Services;

public class AdvanceService
{
    private readonly GrpcAdvanceService.GrpcAdvanceServiceClient _client;

    public AdvanceService(GrpcService grpcService)
    {
        _client = new(grpcService.CallInvoker);
    }

    public async Task<(bool Success, string Message)> SendAdvanceAsync(GrpcAdvanceRequest request)
    {
        try
        {
            var response = await _client.SendAdvanceAsync(request);
            return (response.Success, response.Message);
        }
        catch (RpcException ex)
        {
            return (false, $"gRPC error: {ex.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
