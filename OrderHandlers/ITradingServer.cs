using Grpc.Core;
using Trading;

namespace TradingServer.Handlers
{
    public interface ITradingServer
    {
        Task<OrderResponse> ProcessOrderAsync(OrderRequest orderRequest, ServerCallContext context);
    }
}
