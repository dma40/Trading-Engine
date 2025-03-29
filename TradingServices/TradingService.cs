using Grpc.Net.Client;
using Trading;
using Microsoft.Extensions.Hosting;

namespace TradingServer.Services
{
    public class TradingService: ITradingService // , IHostedService
    {
        // In the TradingServer class, this is what will happen:
        // - Make a PlaceOrder class
        // - The PlaceOrder method will call this one - in the place of await Task.Delay there will be a step to add the order to 
        // the orderbook.
        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            if (string.IsNullOrEmpty(request.Id.ToString()))
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "You have placed a invalid order"
                };
            }

            await Task.Delay(200); // maybe add a Order object containing the requisite information to the Orderbook

            return new OrderResponse
            {
                Id = request.Id,
                Status = 500,
                Message = "Order placed successfully!"
            };
        }
    }
    // Success! Now after this is done I can work on OrderHandlers.
}

