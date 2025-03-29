using Trading;
using Microsoft.Extensions.Hosting;

namespace TradingServer.Services
{
    public interface ITradingService
    {
        Task<OrderResponse> ProcessOrderAsync(OrderRequest request);
    }
}