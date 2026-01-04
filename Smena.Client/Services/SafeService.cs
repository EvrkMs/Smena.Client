using Grpc.Net.Client;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public class SafeService
{
    private readonly GrpcSafeService.GrpcSafeServiceClient Client;
    public long CurrentSafe 
    {
        get => field;
        private set
        {
            field = value;
            SafeChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<long>? SafeChanged;

    public SafeService(GrpcChannel _channel)
    {
        Client = new(_channel);

        LoadOrReloadCurrentSafe();
    }

    public void LoadOrReloadCurrentSafe()
    {
        var amount = Client.CurrentSafe(new GrpcRequest());

        CurrentSafe = amount.Current;
    }
}
