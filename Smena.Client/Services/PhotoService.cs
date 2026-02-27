using Grpc.Core;
using Host.Grpc.Services.SendPhoto;

namespace Smena.Client.Services;

public class PhotoService
{
    private readonly SendPhotoService.SendPhotoServiceClient _client;

    public PhotoService(GrpcService grpcService)
    {
        _client = new(grpcService.CallInvoker);
    }

    public async Task<(bool Success, string Message, string? SessionKey)> RequestPhotosAsync(
        string employeeId,
        Action<string>? progress,
        CancellationToken ct = default)
    {
        var request = new RequestPhotosRequest { EmployeeId = employeeId };
        using var call = _client.RequestPhotos(request, cancellationToken: ct);

        await foreach (var update in call.ResponseStream.ReadAllAsync(ct))
        {
            if (update.Start != null)
            {
                progress?.Invoke(update.Start.Message);
            }
            else if (update.RequestSent != null)
            {
                progress?.Invoke(update.RequestSent.Message);
            }
            else if (update.PhotosReceived != null)
            {
                progress?.Invoke($"Received {update.PhotosReceived.ReceivedCount} photos");
            }
            else if (update.PhotosReady != null)
            {
                return (true, "Photos ready", update.PhotosReady.SessionKey);
            }
            else if (update.Timeout != null)
            {
                return (false, update.Timeout.Message, null);
            }
            else if (update.Error != null)
            {
                return (false, update.Error.Message, null);
            }
        }

        return (false, "Photo stream ended unexpectedly", null);
    }
}
