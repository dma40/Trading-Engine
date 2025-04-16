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

        public MatchResult match(Order order) 
        {   
            Lock _orderLock = new(); 

            MatchResult result = new MatchResult();

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
                
                    if (order.remainingQuantity() > 0)
                    {
                        addOrder(order);
                    }
                }
                // initial final prices are going to be determined in each of the loops; depends on buy, sell side;
                // we also want to record what happened to each of the orders 

                return result;
            }
        }
    

        private readonly Security _security;
    }
}