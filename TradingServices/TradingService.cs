
using TradingServer.Handlers;

namespace Trading
{
    public class TradingClient: TradingService.TradingServiceBase
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
        // The point is that when this method is called,
        // the message will be relayed to the TradingServer which will then
        // add the order to the orderbook, which will store all order records.
    }
}

