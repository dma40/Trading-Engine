using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingEngine
    {
        async Task addOrder(Order order)
        {
            await Task.Delay(200);
        }

        async Task removeOrder(CancelOrder cancel)
        {
            await Task.Delay(200);
        }

        async Task modifyOrder(ModifyOrder modify)
        {
            await Task.Delay(200);
        }

        Trades match(Order order);
    }
}