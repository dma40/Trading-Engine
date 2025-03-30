using Grpc.Net.Client;
using TradingServer.Handlers;
using Microsoft.Extensions.Hosting;

using Grpc.Core;

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
        // In the TradingServer class, this is what will happen:
        // - Make a PlaceOrder class
        // - The PlaceOrder method will call this one - in the place of await Task.Delay there will be a step to add the order to 
        // the orderbook.
    }
}

