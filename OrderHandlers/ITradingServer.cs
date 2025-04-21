using Trading;

namespace TradingServer.Handlers
{
    public interface ITradingServer
    {
        Task<OrderResponse> ProcessOrderAsync(OrderRequest orderRequest);
    }
}
