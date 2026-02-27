using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Host.Grpc.Common;
using Host.Grpc.Services.Safe;
using System.Threading;

namespace Smena.Client.Services;

public sealed class SafeService : IDisposable
{
    private readonly GrpcSafeService.GrpcSafeServiceClient _client;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _streamTask;
    private long _currentSafe;
    private bool _disposed;

    public event EventHandler<long>? SafeChanged;

    public SafeService(GrpcService grpcService)
    {
        _client = new GrpcSafeService.GrpcSafeServiceClient(grpcService.CallInvoker);

        // set initial value (best-effort)
        TryInitCurrentSafe();

        // start background stream
        _streamTask = StartSafeStreamAsync();
    }

    public long CurrentSafe => Interlocked.Read(ref _currentSafe);

    public long LoadOrReloadCurrentSafe()
    {
        var amount = _client.CurrentSafe(new Empty());
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

        _cts.Cancel();
        try
        {
            _streamTask.Wait(1000);
        }
        catch
        {
            // ignore
        }
        _cts.Dispose();
    }

    private void UpdateCurrentSafe(long value)
    {
        Interlocked.Exchange(ref _currentSafe, value);
        SafeChanged?.Invoke(this, value);
    }

    private void TryInitCurrentSafe()
    {
        try
        {
            LoadOrReloadCurrentSafe();
        }
        catch
        {
            // ignore init failure; stream will eventually update
        }
    }

    private async Task StartSafeStreamAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var call = _client.SubscribeSafe(new Empty(), cancellationToken: _cts.Token);

                await foreach (var msg in call.ResponseStream.ReadAllAsync(_cts.Token))
                {
                    UpdateCurrentSafe(msg.Current);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                break;
            }
            catch
            {
                // retry after short delay
                try
                {
                    await Task.Delay(1000, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
