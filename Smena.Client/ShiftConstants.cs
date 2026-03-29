namespace Smena.Client;

internal static class ShiftConstants
{
    /// <summary>Начальная сумма наличных в кассе на начало смены.</summary>
    public const int InitialCashRegister = 1000;

    /// <summary>Максимальное количество сотрудников в смене.</summary>
    public const int MaxEmployeesPerShift = 3;

    /// <summary>Максимальное суммарное количество часов в смене.</summary>
    public const int MaxHoursPerShift = 12;

    /// <summary>Максимальная длина ввода суммы (цифры).</summary>
    public const int MaxAmountDigits = 6;

    /// <summary>Максимальная длина ввода часов (цифры).</summary>
    public const int MaxHoursDigits = 2;

    /// <summary>Таймаут ожидания фото через gRPC-стрим.</summary>
    public static readonly TimeSpan PhotoStreamTimeout = TimeSpan.FromMinutes(5);

    /// <summary>Общий таймаут HTTP-клиента для gRPC (фото + Telegram).</summary>
    public static readonly TimeSpan GrpcHttpTimeout = TimeSpan.FromMinutes(6);
}
