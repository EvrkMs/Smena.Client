using Grpc.Net.Client;
using Host.Grpc.Services.Expense;

namespace Smena.Client.Services;

public class ExpenseService(GrpcChannel channel) : GrpcExpense.GrpcExpenseClient
{
    private readonly GrpcExpense.GrpcExpenseClient _client = new(channel);
}
