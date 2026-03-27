using Google.Protobuf.WellKnownTypes;
using Host.Grpc.Common;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public sealed class SafeService : IDisposable
{
    private readonly GrpcSafeService.GrpcSafeServiceClient _client;
    private long _currentSafe;
    private bool _disposed;

    public event EventHandler<long>? SafeChanged;

    public SafeService(GrpcService grpcService)
    {
        _client = new GrpcSafeService.GrpcSafeServiceClient(grpcService.CallInvoker);
        // NOTE: init is now async — call RefreshCurrentSafeAsync() from OnShown.
    }

    public long CurrentSafe => Interlocked.Read(ref _currentSafe);

    public long LoadOrReloadCurrentSafe()
    {
        var amount = _client.CurrentSafe(new Empty());
        UpdateCurrentSafe(amount.Current);
        return _currentSafe;
    }

    public async Task<long> RefreshCurrentSafeAsync(CancellationToken ct = default)
    {
        var amount = await _client.CurrentSafeAsync(new Empty(), cancellationToken: ct);
        UpdateCurrentSafe(amount.Current);
        return _currentSafe;
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private void UpdateCurrentSafe(long value)
    {
        var previous = Interlocked.Exchange(ref _currentSafe, value);
        if (previous == value)
        {
            return;
        }

        SafeChanged?.Invoke(this, value);
    }
}
