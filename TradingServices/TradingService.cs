using Grpc.Net.Client;
using TradingServer.Handlers;
using Microsoft.Extensions.Hosting;

using Trading;
using Grpc.Core;

namespace TradingServer.Services
{
    public class TradingService // , IHostedService
    {
        private readonly ITradingServer _tradingServer;

        public TradingService(ITradingServer tradingServer)
        {
            _tradingServer = tradingServer;
        }

        public async Task<OrderResponse> PlaceOrder(OrderRequest request, ServerCallContext context)
        {
            var response = await _tradingServer.ProcessOrderAsync(request);
            return response;
        }
        // In the TradingServer class, this is what will happen:
        // - Make a PlaceOrder class
        // - The PlaceOrder method will call this one - in the place of await Task.Delay there will be a step to add the order to 
        // the orderbook.
    }
    // Success! Now after this is done work on OrderHandlers can continue.
}

