using Grpc.Core;
using Host.Grpc.Services.Raport;
using System;

namespace Smena.Client.Services;

public class RaportService(GrpcService grpcService)
{
    private readonly GrpcRaportService.GrpcRaportServiceClient _client = new(grpcService.CallInvoker);

    public async Task<(bool Success, string Message)> SendRaportAsync(GrpcRaportRequest request)
    {
        try
        {
            var response = await _client.SendRaportAsync(
                request,
                deadline: DateTime.UtcNow.AddMinutes(5));
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
