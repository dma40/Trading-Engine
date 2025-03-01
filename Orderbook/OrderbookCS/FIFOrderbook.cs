using TradingServer.Instrument;
using TradingServer.Orders;

// For reference: Pro Rata and FIFO can both be used in the context
// of limit orderbooks. This means that they can both inherit off of LimitOrderbook

// For now this is a limit orderbook using the FIFO algorithm for matching orders

namespace TradingServer.OrderbookCS
{
    public class FIFOrderbook: Orderbook, IMatchingOrderbook
    {
        // This orderbook will make use of the limits defined
        // Maybe do more work so that we can support multiple order types
        public FIFOrderbook(Security security): base(security)
        {
            // we can match only if there is something with a price higher than a sell price;
            // not when it is the same price
            _security = security;
        }

        public MatchResult match()
        {
            MatchResult result = new MatchResult();

            if (getAskLimits().Count == 0 || getBidLimits().Count == 0)
            {
                throw new InvalidOperationException("Orders cannot be matched because there are either no buy orders or no sell orders to match");
            }

            // note the matching engine terminates when there are no more matches to complete
            
            while (canMatch()) 
            // always looks out for matches whenever possible; ends when
            // no more matches can be executed
            {
                // pair the lowest + highest in the next step
                Limit min = getAskLimits().Min; // minimum nonempty limit
                Limit max = getBidLimits().Max; // maximum available nonempty limit

                OrderbookEntry askPtr = min.head;
                OrderbookEntry bidPtr = max.head;

                while (askPtr != null && bidPtr != null)
                {
                    uint buyQuantity = bidPtr.CurrentOrder.CurrentQuantity;
                    uint sellQuantity = askPtr.CurrentOrder.CurrentQuantity;

                    if (buyQuantity > sellQuantity)
                    {
                        // this is ok because CurrentQuantity is the amount of unfilled orders left
                        // fill a certain number of orders
                        bidPtr.CurrentOrder.DecreaseQuantity(sellQuantity);
                        askPtr.CurrentOrder.DecreaseQuantity(sellQuantity);

                        askPtr = askPtr.next;
                        removeOrder(askPtr.previous.CurrentOrder.OrderID, askPtr.previous, _orders);
                    }

                    else if (sellQuantity > buyQuantity)
                    {
                        bidPtr.CurrentOrder.DecreaseQuantity(buyQuantity);
                        askPtr.CurrentOrder.DecreaseQuantity(buyQuantity);

                        bidPtr = bidPtr.next;
                        removeOrder(bidPtr.previous.CurrentOrder.OrderID, bidPtr.previous, _orders);
                    }

                    else 
                    {
                        bidPtr.CurrentOrder.DecreaseQuantity(buyQuantity);
                        askPtr.CurrentOrder.DecreaseQuantity(sellQuantity);

                        askPtr = askPtr.next;
                        bidPtr = bidPtr.next;

                        removeOrder(askPtr.previous.CurrentOrder.OrderID, askPtr.previous, _orders);
                        removeOrder(bidPtr.previous.CurrentOrder.OrderID, bidPtr.previous, _orders);
                    }

                    result.addTransaction(askPtr);
                }
            }
            return result;
        }

        private readonly Security _security;
    }
}