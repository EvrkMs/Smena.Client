using Grpc.Net.Client;
class GrpcService(string address)
{
    public GrpcChannel Channel => GrpcChannel.ForAddress(address);
}
