using Grpc.Core;
using GrpcServices;
using TradingServer.Logging;
using TradingServer.Orderbook;

namespace TradingServices.Services;

public class TradingService : TradingService.TradingServerBase
{
    private readonly ITextLogger<TradingService> _logger;
    public TradingService(TextLogger<TradingService> logger)
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
