using Grpc.Net.Client;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public class SafeService
{
    private readonly GrpcSafeService.GrpcSafeServiceClient Client;
    public long CurrentSafe => LoadOrReloadCurrentSafe();

    public event EventHandler<long>? SafeChanged;

    public SafeService(GrpcChannel _channel)
    {
        Client = new(_channel);
    }

    public long LoadOrReloadCurrentSafe()
    {
        var amount = Client.CurrentSafe(new GrpcRequest());

        return amount.Current;
    }
}
