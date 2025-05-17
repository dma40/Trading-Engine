using Grpc.Core;
using TradingServer.Services;

namespace Trading
{
    public class TradingClient: TradingService.TradingServiceBase, ITradingServer
    {
        private readonly ITradingServer _tradingServer; 

        public TradingClient(ITradingServer tradingServer)
        {
            _tradingServer = tradingServer;
        }

        public override async Task<OrderResponse> ProcessOrderAsync(OrderRequest request, ServerCallContext context)
        {
            var response = await _tradingServer.ProcessOrderAsync(request, context);
            return response;
        }
    }
}

