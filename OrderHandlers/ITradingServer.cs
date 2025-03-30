
using Trading;

namespace TradingServer.Handlers
{
    // this creates shared functionalities that TradingServer can use
    // make sure to modify this so that it works properly in the future - 
    // it's necessary to make sure that this project can have gRPC .proto files 
    // compiled into C# code
    public interface ITradingServer
    {
        Task Run(CancellationToken token);

        Task<OrderResponse> ProcessOrderAsync(OrderRequest orderRequest);
    }
}
