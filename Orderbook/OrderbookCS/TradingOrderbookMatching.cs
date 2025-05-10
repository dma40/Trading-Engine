using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        private static readonly List<int> ImmediateHandleTypes = [3, 5, 7, 9, 11, 12, 13, 14];
        private readonly Trades _trades;

        public new Trades match(Order order) 
        {   
            int type = (int) order.OrderType;
            Trades result = new Trades();

            lock (_ordersLock)
            {
                if (ImmediateHandleTypes.Contains(type))
                {
                    result = base.match(order);
                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                        result = base.match(order);  

                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        base.addOrder(order);

                    else
                        order.Dispose();
                }

                else 
                {
                    result = base.match(order);
                
                    if (order.CurrentQuantity > 0)
                        base.addOrder(order); 
                }
                
                _trades.addTransactions(result);

                return result;
            }
        }
    }
}