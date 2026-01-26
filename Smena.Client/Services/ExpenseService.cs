using Grpc.Core;
using Host.Grpc.Services.Expense;

namespace Smena.Client.Services;

public class ExpenseService
{
    private readonly GrpcExpense.GrpcExpenseClient _client;

    public ExpenseService(GrpcService grpcService)
    {
        _client = new(grpcService.CallInvoker);
    }

    public async Task<(bool Success, string Message)> AddExpenseOperationAsync(GrpcExpenseAdd request)
    {
        try
        {
            var response = await _client.AddExpenseOperationAsync(request);
            return (response.Success, response.Message);
        }
        catch (RpcException ex)
        {
            return (false, $"gRPC error: {ex.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
