using Grpc.Core;
using GrpcServices;
using TradingServer.Logging;
using TradingServer.Orderbook;

// make sure to re-evaluate this 

namespace TradingServer.Client
{
    public class TradingClient
    {
        private readonly TradingService.TradingServiceClient _client;

        public TradingClient(string address = "localhost:12000")
        {
            var channel = GrpcChannel.ForAddress("https://{address}");
            _client = new TradingService.TradingServiceClient(channel);
        }

        public async Task PlaceOrderAsync(int quantity, float price, uint operation)
        {
            // var request = new OrderRequest ... followed by initialization
            // find a way to submit this to the orderbook

            // use server context related stuff to our advantage. When we send this out,
            // the request will exist in the server context and will definitely be recieved by the TradingServer on the other end.
        }
    }
}
