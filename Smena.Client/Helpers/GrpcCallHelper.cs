using Grpc.Core;
using Host.Grpc.Common;

namespace Smena.Client.Helpers;

internal static class GrpcCallHelper
{
    /// <summary>
    /// Wraps a gRPC call that returns <see cref="BoolResponse"/> with standard error handling.
    /// </summary>
    public static async Task<(bool Success, string Message)> CallAsync(
        Func<Task<BoolResponse>> call)
    {
        try
        {
            var response = await call();
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

    /// <summary>
    /// Wraps a gRPC call with custom result extraction.
    /// </summary>
    public static async Task<TResult> CallAsync<TResponse, TResult>(
        Func<Task<TResponse>> call,
        Func<TResponse, TResult> onSuccess,
        Func<string, TResult> onError)
    {
        try
        {
            var response = await call();
            return onSuccess(response);
        }
        catch (RpcException ex)
        {
            return onError($"gRPC: {ex.StatusCode} - {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            return onError(ex.Message);
        }
    }
}
