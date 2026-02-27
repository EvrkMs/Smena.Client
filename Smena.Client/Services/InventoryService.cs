using Grpc.Core;
using Host.Grpc.Services.Inventory;

namespace Smena.Client.Services;

public class InventoryService
{
    private readonly GrpcInventoryService.GrpcInventoryServiceClient _client;

    public InventoryService(GrpcService grpcService)
    {
        _client = new(grpcService.CallInvoker);
    }

    public async Task<(bool Success, string Message)> SendInventoryAsync(GrpcInventoryRequest request)
    {
        try
        {
            var response = await _client.SendInventoryAsync(request);
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
