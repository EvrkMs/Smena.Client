using Host.Grpc.Services.Raport;
using Smena.Client.Helpers;

namespace Smena.Client.Services;

public class RaportService(GrpcService grpcService)
{
    private readonly GrpcRaportService.GrpcRaportServiceClient _client = new(grpcService.CallInvoker);

    public Task<(bool Success, string Message)> SendRaportAsync(
        GrpcRaportRequest request,
        CancellationToken ct = default)
        => GrpcCallHelper.CallAsync(() => _client.SendRaportAsync(
            request,
            deadline: DateTime.UtcNow.Add(ShiftConstants.PhotoStreamTimeout),
            cancellationToken: ct).ResponseAsync);
}
