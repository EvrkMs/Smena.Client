using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Host.Grpc.Common;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public sealed class SafeService : IDisposable
{
    private readonly GrpcSafeService.GrpcSafeServiceClient _client;
    private long _currentSafe;
    private bool _disposed;
    private CancellationTokenSource? _streamCts;

    public event EventHandler<long>? SafeChanged;

    public SafeService(GrpcService grpcService)
    {
        _client = new GrpcSafeService.GrpcSafeServiceClient(grpcService.CallInvoker);
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

    /// <summary>
    /// Starts a background gRPC server-stream that receives safe value updates in real time.
    /// The stream automatically reconnects on failure. Call <see cref="Dispose"/> to stop.
    /// </summary>
    public void StartSubscription()
    {
        if (_streamCts != null) return;

        _streamCts = new CancellationTokenSource();
        _ = RunSubscriptionLoopAsync(_streamCts.Token);
    }

    private async Task RunSubscriptionLoopAsync(CancellationToken ct)
    {
        const int reconnectDelayMs = 3000;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var call = _client.SubscribeSafe(new Empty(), cancellationToken: ct);

                await foreach (var update in call.ResponseStream.ReadAllAsync(ct))
                {
                    UpdateCurrentSafe(update.Current);
                }
            }
            catch (RpcException) when (!ct.IsCancellationRequested)
            {
                // Server dropped connection — wait and retry.
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch when (!ct.IsCancellationRequested)
            {
                // Unexpected error — wait and retry.
            }

            if (!ct.IsCancellationRequested)
            {
                try { await Task.Delay(reconnectDelayMs, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
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

        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _streamCts = null;
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
