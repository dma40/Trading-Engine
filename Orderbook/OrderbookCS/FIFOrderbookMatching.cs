using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private Trades _trades;

        public new Trades match(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = base.match(order);
                        order.Dispose();
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill)
                {
                    result = base.match(order);
                    order.Dispose();
                } 

                else if (order.OrderType == OrderTypes.Market)
                {
                    result = base.match(order);
                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        base.addOrder(order);
                }

                else 
                {
                    result = base.match(order);
                
                    if (order.CurrentQuantity > 0)
                    {
                        base.addOrder(order);
                    }

                    else 
                    {
                        order.Dispose();
                    }
                }

                return result;
            }
        }
    }
}