using Google.Protobuf.WellKnownTypes;
using Host.Grpc.Services.Constants;
using Smena.Client.Helpers;

namespace Smena.Client.Services;

public sealed record ShiftConstantsSnapshot(
    int InitialCashRegister,
    int MaxEmployeesPerShift,
    int MaxHoursPerShift,
    int MaxAmountDigits,
    int MaxHoursDigits);

/// <summary>
/// Бизнес-константы смены с сервера (единый источник — ShiftRules на сервере,
/// endpoint GrpcConstantsService). Локальный ShiftConstants — только фолбэк на
/// случай недоступности сервера; неудачный ответ не кэшируется, чтобы при
/// восстановлении связи подтянуть серверные значения.
/// </summary>
public class ConstantsService(GrpcService grpcService)
{
    private readonly GrpcConstantsService.GrpcConstantsServiceClient _client = new(grpcService.CallInvoker);
    private ShiftConstantsSnapshot? _cached;

    public async Task<ShiftConstantsSnapshot> GetAsync(CancellationToken ct = default)
    {
        if (_cached != null)
        {
            return _cached;
        }

        try
        {
            var response = await _client.GetShiftConstantsAsync(
                new Empty(),
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: ct);

            _cached = new ShiftConstantsSnapshot(
                response.InitialCashRegister,
                response.MaxEmployeesPerShift,
                response.MaxHoursPerShift,
                response.MaxAmountDigits,
                response.MaxHoursDigits);
            return _cached;
        }
        catch (Exception ex)
        {
            ErrorLog.Write("GetShiftConstants", ex);
            return new ShiftConstantsSnapshot(
                ShiftConstants.InitialCashRegister,
                ShiftConstants.MaxEmployeesPerShift,
                ShiftConstants.MaxHoursPerShift,
                ShiftConstants.MaxAmountDigits,
                ShiftConstants.MaxHoursDigits);
        }
    }
}
