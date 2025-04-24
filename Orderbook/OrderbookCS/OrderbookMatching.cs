using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IRetrievalOrderbook, IDisposable
    {
        public virtual bool canFill(Order order)
        {
            if (order.isBuySide)
            {
                uint askQuantity = 0;

                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry askHead = ask.head;

                        while (askHead != null)
                        {
                            askQuantity += askHead.CurrentOrder.CurrentQuantity;
                            askHead = askHead.next;
                        }
                    }
                }
                return askQuantity >= order.CurrentQuantity;
            }

            else
            {
                uint bidQuantity = 0;
                
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        OrderbookEntry bidHead = bid.head;

                        while (bidHead != null)
                        {
                            bidQuantity += bidHead.CurrentOrder.CurrentQuantity;
                            bidHead = bidHead.next;
                        }
                    }
                }

                return bidQuantity >= order.CurrentQuantity;
            }
        }

        public virtual Trades match(Order order) // redesign the whole thing
        {
            Trades result = new Trades();

            if (order.isBuySide)
            {
                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        // work out new logic here, this needs to be fixed
                        // needs to check for duplicated/corrupted refs
                    }
                }
            }

            else 
            {
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        // work out new logic here, this needs to be fixed
                        // need to ensure safety against duplicated/corrupted refs
                    }
                }
            }

            return result;
        }

        /*
        public Trades matchIncoming(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
                {
                    throw new InvalidOperationException("In general, we attempt to match non-stop orders when they come in "
                    + "- we add them to internal queues instead with addOrder.");
                }

                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = match(order);
                        order.Dispose();
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill || order.OrderType == OrderTypes.Market)
                {
                    result = match(order);
                    order.Dispose();
                } 

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        addOrder(order);
                }

                else if (order.OrderType == OrderTypes.StopMarket || order.OrderType == OrderTypes.StopLimit
                        || order.OrderType == OrderTypes.TrailingStop
                        || order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose
                        ||order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
                {
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

        public void processIncoming(Order order) // review this later, is this necessary?
        {
            if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
            {
                addOrder(order);
            }

            else 
            {
                matchIncoming(order);
            }
        } 
    }
    */
    }
}