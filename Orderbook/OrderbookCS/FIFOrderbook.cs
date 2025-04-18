using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class FIFOrderbook: Orderbook, IMatchingOrderbook
    {
        public FIFOrderbook(Security security): base(security)
        {
            _security = security;
        }

        public Trades match(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = fill(order);
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill)
                {
                    result = fill(order);
                } 

                else if (order.OrderType == OrderTypes.Market)
                {
                    result = fill(order);
                }

                else 
                {
                    result = fill(order);
                
                    if (order.CurrentQuantity > 0)
                    {
                        addOrder(order);
                    }
                    // addOrder(order);
                }

                return result;
            }
        }
    

        private readonly Security _security;
    }
}