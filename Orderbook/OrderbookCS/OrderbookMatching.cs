using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IRetrievalOrderbook, IDisposable
    {
        protected virtual bool canFill(Order order)
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

        protected virtual Trades match(Order order) 
        {
            Trades result = new Trades();

            if (order.isBuySide)
            {
                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry head = ask.head;

                        while (head != null)
                        {
                            var entry = head.CurrentOrder;

                            //if (some condition)
                            //{
                            // (make a new cancel order, cancel one of the orders)
                            // executeTrade(order1, order2)

                            // head = head.next;
                            // remove the other order if necessary
                            //}

                            // else
                            // {
                            //  advance the order pointer + keep trying to fill; if filled up, break
                            // }
                        }
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
    }
}