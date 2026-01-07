using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services.Inventory;

namespace Smena.Client.Services;

public class InventoryService
{
    private readonly GrpcInventoryService.GrpcInventoryServiceClient _client;

    public InventoryService(GrpcChannel channel)
    {
        _client = new(channel);
    }

    public async Task<(bool Success, string Message)> SendInventoryAsync(GrpcInventoryRequest request)
    {
        try
        {
            var response = await _client.SendInventoryAsync(request);
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