using Google.Protobuf.WellKnownTypes;
using Host.Grpc.Common;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public class SafeService(GrpcService grpcService)
{
    private readonly GrpcSafeService.GrpcSafeServiceClient _client = new(grpcService.CallInvoker);

    public event EventHandler<long>? SafeChanged;

    public long CurrentSafe => LoadOrReloadCurrentSafe();

    public long LoadOrReloadCurrentSafe()
    {
        var amount = _client.CurrentSafe(new Empty());
        return amount.Current;
    }

    public async Task<BoolResponse> AddOperationSafeAsync(SafeOperationAdd req, CancellationToken ct = default)
    {
        try
        {
            var call = await _client.AddSafeOperationAsync(req, cancellationToken: ct);
            return call ?? new BoolResponse { Success = false, Message = "Server did not respond." };
        }
        catch (Exception ex)
        {
            return new BoolResponse { Success = false, Message = ex.Message };
        }
    }
}
