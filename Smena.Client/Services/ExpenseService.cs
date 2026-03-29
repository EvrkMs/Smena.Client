using Host.Grpc.Services.Expense;
using Smena.Client.Helpers;

namespace Smena.Client.Services;

public class ExpenseService(GrpcService grpcService)
{
    private readonly GrpcExpense.GrpcExpenseClient _client = new(grpcService.CallInvoker);

    public Task<(bool Success, string Message)> AddExpenseOperationAsync(GrpcExpenseAdd request)
        => GrpcCallHelper.CallAsync(() => _client.AddExpenseOperationAsync(request).ResponseAsync);
}
