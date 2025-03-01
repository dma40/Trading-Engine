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
                    uint q1 = bidPtr.CurrentOrder.CurrentQuantity;
                    uint q2 = askPtr.CurrentOrder.CurrentQuantity;
                    
                    if (q1 > q2)
                    {
                        // this is ok because CurrentQuantity is the amount of unfilled orders left
                        bidPtr.CurrentOrder.DecreaseQuantity(q1 - q2);
                        askPtr.CurrentOrder.DecreaseQuantity(q2);

                        askPtr = askPtr.next;
                        removeOrder(askPtr.previous.CurrentOrder.OrderID, askPtr.previous, _orders);
                    }

                    else if (q2 > q1)
                    {
                        bidPtr.CurrentOrder.DecreaseQuantity(q1);
                        askPtr.CurrentOrder.DecreaseQuantity(q2 - q1);

                        bidPtr = bidPtr.next;
                        removeOrder(bidPtr.previous.CurrentOrder.OrderID, bidPtr.previous, _orders);
                    }

                    else 
                    {
                        bidPtr.CurrentOrder.DecreaseQuantity(q1);
                        askPtr.CurrentOrder.DecreaseQuantity(q2);

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