using Grpc.Net.Client;
using Host.Grpc.Services.Requests;
using Host.Grpc.Services.Responses;
using Host.Grpc.Services.Safe;

namespace Smena.Client.Services;

public class SafeService(GrpcChannel _channel)
{
    private readonly GrpcSafeService.GrpcSafeServiceClient Client = new(_channel);
    public long CurrentSafe => LoadOrReloadCurrentSafe();

    public event EventHandler<long>? SafeChanged;

    public long LoadOrReloadCurrentSafe()
    {
        var amount = Client.CurrentSafe(new EmptyRequest());

        return amount.Current;
    }

    public async Task<BoolResponse> AddOperationSafeAsync(GrpcSafeOperation req, CancellationToken ct = default)
    {
        try
        {
            var call = await Client.SafeOperationAsync(req, cancellationToken: ct);

            if (call == null)
            {
                return new BoolResponse() { Value = false, Message = "Сервер не дал ответ" };
            }

            return new BoolResponse() { Value = true };
        }
        catch (Exception ex)
        {
            return new BoolResponse() { Value = false, Message = ex.Message };
        }
    }
}
