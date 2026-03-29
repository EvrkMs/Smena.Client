using Host.Grpc.Services.Advance;
using Smena.Client.Helpers;

namespace Smena.Client.Services;

public class AdvanceService(GrpcService grpcService)
{
    private readonly GrpcAdvanceService.GrpcAdvanceServiceClient _client = new(grpcService.CallInvoker);

    public Task<(bool Success, string Message)> SendAdvanceAsync(GrpcAdvanceRequest request)
        => GrpcCallHelper.CallAsync(() => _client.SendAdvanceAsync(request).ResponseAsync);
}
