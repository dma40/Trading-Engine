using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        // private static readonly List<int> ImmediateHandleTypes = [3, 5, 7, 9, 11, 12, 13, 14];
        private readonly Trades _trades;
        /*
        public Trades match(Order order) 
        {   
            int type = (int) order.OrderType;
            Trades result = new Trades();

            lock (_ordersLock)
            {
                if (ImmediateHandleTypes.Contains(type))
                {
                    result = orderbook.match(order);
                    result.addTransactions(_hidden.match(order));
                }

                else if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (hasEligibleOrderCount(order))
                    {
                        result = orderbook.match(order);
                        result.addTransactions(_hidden.match(order));
                    }
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!orderbook.canMatch(order))
                    {
                        orderbook.addOrder(order);
                    }
                }

                else
                {
                    result = orderbook.match(order);
                    result.addTransactions(_hidden.match(order));

                    if (order.CurrentQuantity > 0)
                    {
                        if (order.isHidden)
                        {
                            _hidden.addOrder(order);
                        }

                        else
                        {
                            orderbook.addOrder(order);
                        }
                    }
                }

                _trades.addTransactions(result);
               
                return result;
            }
        }
        */

        public Trades match(Order order)
        {
            Trades result = _strategies.match(order);
            _trades.addTransactions(result);
            return result;
        }

        public bool hasEligibleOrderCount(Order order)
        {
            return orderbook.getEligibleOrderCount(order) + _hidden.getEligibleOrderCount(order) >= order.CurrentQuantity;
        }
    }
}