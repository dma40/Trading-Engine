using Grpc.Net.Client;
using TradingServer.Handlers;
using Microsoft.Extensions.Hosting;

namespace TradingServer.Services

// make sure to fix the errors seen here
{
    public class TradingService // , IHostedService
    {
        // In the TradingServer class, this is what will happen:
        // - Make a PlaceOrder class
        // - The PlaceOrder method will call this one - in the place of await Task.Delay there will be a step to add the order to 
        // the orderbook.

        /*
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
        */
    }
    // Success! Now after this is done work on OrderHandlers can continue.
}

