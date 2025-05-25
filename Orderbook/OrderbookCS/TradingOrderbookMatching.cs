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
                    //Console.WriteLine("Not a special order type. Starting match");
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

                //Console.WriteLine("Length of result: " + result.recordedTrades.Count);
                //Console.WriteLine("Number of trades: " + _trades.recordedTrades.Count);
               
                return result;
            }
        }

        public bool hasEligibleOrderCount(Order order)
        {
           return orderbook.getEligibleOrderCount(order) + _hidden.getEligibleOrderCount(order) > order.CurrentQuantity;
        }
    }
}