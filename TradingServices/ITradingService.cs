
using Microsoft.Extensions.Hosting;
using TradingServer.Handlers;
using Trading;

namespace TradingServer.Services
{
    public interface ITradingService
    {
    Task<OrderResponse> ProcessOrderAsync(OrderRequest request);
    }
}