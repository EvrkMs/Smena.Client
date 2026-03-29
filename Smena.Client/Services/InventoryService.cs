using Host.Grpc.Services.Inventory;
using Smena.Client.Helpers;

namespace Smena.Client.Services;

public class InventoryService(GrpcService grpcService)
{
    private readonly GrpcInventoryService.GrpcInventoryServiceClient _client = new(grpcService.CallInvoker);

    public Task<(bool Success, string Message)> SendInventoryAsync(GrpcInventoryRequest request)
        => GrpcCallHelper.CallAsync(() => _client.SendInventoryAsync(request).ResponseAsync);
}
