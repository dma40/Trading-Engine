using Grpc.Core;
using GrpcServices;
using TradingServer.Logging;
using TradingServer.Orderbook;

namespace GrpcServices.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ITextLogger<GreeterService> _logger;
    public GreeterService(TextLogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
