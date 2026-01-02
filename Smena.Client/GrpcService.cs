using Grpc.Net.Client;
class GrpcService
{
    public GrpcChannel Channel { get; }
    public GrpcService(string address = "https://localhost:5001")
    {
        Channel = GrpcChannel.ForAddress(address);
    }
}
