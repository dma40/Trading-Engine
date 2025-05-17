using Grpc.Core;
using Trading;

namespace TradingServer.Services
{
    public interface ITradingServer
    {
        Task<OrderResponse> ProcessOrderAsync(OrderRequest orderRequest, ServerCallContext context);
    }
}
