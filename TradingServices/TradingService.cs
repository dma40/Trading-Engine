
using TradingServer.Handlers;

namespace Trading
{
    public class TradingClient: TradingService.TradingServiceBase, ITradingServer
    {
        private readonly ITradingServer _tradingServer; 

        public TradingClient(ITradingServer tradingServer)
        {
            _tradingServer = tradingServer;
        }

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            var response = await _tradingServer.ProcessOrderAsync(request);
            return response;
        }
    }
}

