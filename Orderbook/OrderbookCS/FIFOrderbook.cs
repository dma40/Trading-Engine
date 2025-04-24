using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook
    {
        public MatchingOrderbook(Security security): base(security)
        {
            _security = security;
        }

        public override Trades match(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = match(order);
                        order.Dispose();
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill)
                {
                    result = match(order);
                    order.Dispose();
                } 

                else if (order.OrderType == OrderTypes.Market)
                {
                    result = match(order);
                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        addOrder(order);
                }

                else 
                {
                    result = match(order);
                
                    if (order.CurrentQuantity > 0)
                    {
                        addOrder(order);
                    }

                    else 
                    {
                        order.Dispose();
                    }
                }

                return result;
            }
        }
    
        private readonly Security _security;
    }
}