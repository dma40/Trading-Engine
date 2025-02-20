using TradingServer.Instrument;
using TradingServer.Orders;

// For reference: Pro Rata and FIFO can both be used in the context
// of limit orderbooks. This means that they can both inherit off of LimitOrderbook

// For now this is a limit orderbook using the FIFO algorithm for matching orders
namespace TradingServer.Orderbook
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
            
            while (canMatch()) // always looks out for matches whenever possible; ends when
            // no more matches can be executed
            {
                Limit limit = getAskLimits().Min; // minimum nonempty limit
                Limit other = getBidLimits().Max; // maximum available nonempty limit

                OrderbookEntry askPtr = limit.head;
                OrderbookEntry bidPtr = other.head;

                while (askPtr != null && bidPtr != null)
                {
                    removeOrder(askPtr.CurrentOrder.OrderID, askPtr, _orders);
                    // find a order that can match bidPtr


                    // cancel one of the two orders
                    // removeOrder(askPtr.CurrentOrder.OrderID, askPtr, _orders);
                }

                    // pair lowest + highest
            }
                // find the least thing that can be matched
                // match orders using some algorithm, now that there are two matching prices we can
                // match. How do we match in this instance?
            
            //List<OrderbookEntry> asks = getAskOrders();
            //List<OrderbookEntry> bids = getBidOrders();
            //while (true)
            //{
            //
            //}
            return result;
            // What should be returned in the result?
            // Maybe return a OrderRecord of all matched orders
            // Pool all the bid and ask orders together, and then match them
        }

        private readonly Security _security;
    }
}