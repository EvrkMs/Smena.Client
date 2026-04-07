using Google.Protobuf.WellKnownTypes;
using Host.Grpc.Services.Warehouse;

namespace Smena.Client.Services;

public class WarehouseService(GrpcService grpcService)
{
    private readonly GrpcWarehouseService.GrpcWarehouseServiceClient _client =
        new(grpcService.CallInvoker);

    /// <summary>Возвращает все позиции из кэша сервера.</summary>
    public async Task<(List<GrpcWarehouseItem> Items, string LastRefreshedUtc, string Error)>
        GetAllItemsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetAllItemsAsync(new Empty(), cancellationToken: ct).ResponseAsync;
            return ([.. response.Items], response.LastRefreshedUtc, string.Empty);
        }
        catch (Exception ex)
        {
            return ([], string.Empty, ex.Message);
        }
    }

    /// <summary>Быстрый поиск позиций по имени / артикулу.</summary>
    public async Task<List<GrpcWarehouseItem>> SearchItemsAsync(
        string query, int limit = 20, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.SearchItemsAsync(
                new GrpcWarehouseSearchRequest { Query = query, Limit = limit },
                cancellationToken: ct).ResponseAsync;
            return [.. response.Items];
        }
        catch
        {
            return [];
        }
    }
}
