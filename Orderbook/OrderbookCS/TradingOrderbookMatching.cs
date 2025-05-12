using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private static readonly List<int> ImmediateHandleTypes = [3, 5, 7, 9, 11, 12, 13, 14];
        private readonly Trades _trades;

        public Trades match(Order order) 
        {   
            int type = (int) order.OrderType;
            Trades result = new Trades();

            lock (_ordersLock)
            {
                if (ImmediateHandleTypes.Contains(type))
                {
                    result = orderbook.match(order);
                }

                else if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (orderbook.canFill(order))
                        result = orderbook.match(order);  
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!orderbook.canMatch(order))
                        orderbook.addOrder(order);
                }

                else 
                {
                    result = orderbook.match(order);
                
                    if (order.CurrentQuantity > 0)
                        orderbook.addOrder(order); 
                }
                
                _trades.addTransactions(result);

                return result;
            }
        }
    }
}