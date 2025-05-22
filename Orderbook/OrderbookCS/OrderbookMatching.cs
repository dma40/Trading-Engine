using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        public uint getEligibleOrderCount(Order order)
        {
            uint count = 0;

            if (order.isBuySide)
            {
                foreach (var limit in _askLimits)
                {
                    if (limit.Price <= order.Price)
                    {
                        OrderbookEntry? headPtr = limit.head;
                        while (headPtr != null)
                        {
                            count += headPtr.CurrentOrder.CurrentQuantity;
                            headPtr = headPtr.next;
                        }
                    }
                }
            }

            else
            {
                foreach (var limit in _bidLimits)
                {
                    if (limit.Price >= order.Price)
                    {
                        OrderbookEntry? headPtr = limit.head;
                        while (headPtr != null)
                        {
                            count += headPtr.CurrentOrder.CurrentQuantity;
                            headPtr = headPtr.next;
                        }
                    }
                }
            }
            return count;
        }

        public bool canFill(Order order)
        {
            if (order.isBuySide)
            {
                uint askQuantity = 0;

                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry? askHead = ask.head;

                        while (askHead != null)
                        {
                            askQuantity += askHead.CurrentOrder.CurrentQuantity;
                            askHead = askHead?.next;
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
                        OrderbookEntry? bidHead = bid.head;

                        while (bidHead != null)
                        {
                            bidQuantity += bidHead.CurrentOrder.CurrentQuantity;
                            bidHead = bidHead?.next;
                        }
                    }
                }

                return bidQuantity >= order.CurrentQuantity;
            }
        }

        public Trades match(Order order) 
        {
            if (containsOrder(order.OrderID))
                throw new InvalidOperationException("Cannot match an order already in the orderbook");
                
            Trades result = new Trades();
            List<OrderbookEntry> cancels = new List<OrderbookEntry>();

            if (order.isBuySide)
            {
                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry? head = ask.head;

                        while (head != null)
                        {
                            var entry = head.CurrentOrder;
                            result.addTransaction(executeTrade(order, entry));

                            if (entry.CurrentQuantity > 0)
                                break;

                            else
                            {
                                cancels.Add(head);
                                head = head.next;
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
                        OrderbookEntry? head = bid.head;

                        while (head != null)
                        {
                            var entry = head.CurrentOrder;
                            result.addTransaction(executeTrade(order, entry));

                            if (entry.CurrentQuantity > 0)
                                break;

                            else
                            {
                                cancels.Add(head);
                                head = head.next;
                            }
                        }
                    }
                }
            }

            removeOrders(cancels);
            return result;
        }

        protected virtual Trade executeTrade(Order incoming, Order resting)
        {
            if (incoming.CurrentQuantity > resting.CurrentQuantity)
            {
                var quantity = resting.CurrentQuantity;
                resting.DecreaseQuantity(quantity);
                incoming.DecreaseQuantity(quantity);

                OrderRecord _incoming = new OrderRecord(incoming.OrderID, 0, incoming.Quantity, incoming.Price, resting.Price, incoming.isBuySide, incoming.Username, incoming.SecurityID, 0, 0);
                OrderRecord _resting = new OrderRecord(resting.OrderID, resting.CurrentQuantity, quantity, resting.Price, resting.Price, resting.isBuySide, incoming.Username, incoming.SecurityID, 0, 0);

                return new Trade(_incoming, _resting);
            }

            else 
            {
                var quantity = incoming.CurrentQuantity;
                resting.DecreaseQuantity(quantity);
                incoming.DecreaseQuantity(quantity);

                OrderRecord _incoming = new OrderRecord(incoming.OrderID, 0, incoming.Quantity, incoming.Price, resting.Price, incoming.isBuySide, incoming.Username, incoming.SecurityID, 0, 0);
                OrderRecord _resting = new OrderRecord(resting.OrderID, resting.CurrentQuantity, quantity, resting.Price, resting.Price, resting.isBuySide, incoming.Username, incoming.SecurityID, 0, 0);

                return new Trade(_incoming, _resting);
            }
        }

        public bool canMatch(Order order)
        {
            if (order.isBuySide)
            {
                if (_askLimits.Any() && _askLimits.Min != null && !_askLimits.Min.isEmpty)
                    return order.Price >= _askLimits.Min.Price;
            }

            else
            {
                if (_bidLimits.Any() && _bidLimits.Max != null && !_bidLimits.Max.isEmpty)
                    return order.Price <= _bidLimits.Max.Price;
            }

            return false;
        }
    }
}