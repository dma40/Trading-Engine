using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
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
                            executeTrade(order, head.CurrentOrder);

                            if (head.CurrentOrder.CurrentQuantity > 0)
                            {
                                break;
                            }

                            else 
                            {
                                head = head.next;
                                removeOrder(entry.cancelOrder());
                            }
                        }
                    }
                }
            }

            else 
            {
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        OrderbookEntry head = bid.head;

                        while (head != null)
                        {
                            var entry = head.CurrentOrder;
                            executeTrade(order, head.CurrentOrder);

                            if (head.CurrentOrder.CurrentQuantity > 0)
                            {
                                break;
                            }

                            else 
                            {
                                head = head.next;
                                removeOrder(entry.cancelOrder());
                            }
                        }
                    }
                }
            }

            return result;
        }

        protected virtual void executeTrade(Order incoming, Order resting)
        {
            // this is the helper function; ideally should return a Trade object.

            if (incoming.CurrentQuantity > resting.CurrentQuantity)
            {
                var quantity = resting.CurrentQuantity;
                resting.DecreaseQuantity(quantity);
                incoming.DecreaseQuantity(quantity);
            }

            else 
            {
                var quantity = incoming.CurrentQuantity;
                resting.DecreaseQuantity(quantity);
                incoming.DecreaseQuantity(quantity);
            }
        }
    }
}