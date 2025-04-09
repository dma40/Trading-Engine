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

        public MatchResult match()
        {

            if (getAskLimits().Count == 0 || getBidLimits().Count == 0)
            {
                throw new InvalidOperationException("Orders cannot be matched because either the buy side or the sell side of the orderbook is empty");
            }

            // note the matching engine terminates when there are no more matches to complete
            
            MatchResult result = new MatchResult();

            // investigate this loop, and also the way limits are being
            // added to the buy and sell sides, as well as the remove() method - those could be causing errors
            // always looks out for matches whenever possible; ends when
            // no more matches can be executed
            
                // pair the lowest + highest in the next step
            Limit min = getAskLimits().Min; // minimum nonempty limit
            Limit max = getBidLimits().Max; // maximum available nonempty limit

            OrderbookEntry askPtr = min.head;
            OrderbookEntry bidPtr = max.head;

            while (askPtr != null && bidPtr != null)
            {
                IOrderCore buyOrderCore = new OrderCore(bidPtr.CurrentOrder.OrderID, bidPtr.CurrentOrder.Username, bidPtr.CurrentOrder.SecurityID);
                IOrderCore askOrderCore = new OrderCore(askPtr.CurrentOrder.OrderID, askPtr.CurrentOrder.Username, askPtr.CurrentOrder.SecurityID);
                CancelOrder bidCancel = new CancelOrder(buyOrderCore);
                CancelOrder askCancel = new CancelOrder(askOrderCore);

                uint buyQuantity = bidPtr.CurrentOrder.CurrentQuantity;
                uint sellQuantity = askPtr.CurrentOrder.CurrentQuantity;

                if (buyQuantity > sellQuantity)
                {
                    // this is ok because CurrentQuantity is the amount of unfilled orders left
                    bidPtr.CurrentOrder.DecreaseQuantity(sellQuantity);
                    askPtr.CurrentOrder.DecreaseQuantity(sellQuantity);

                    result.addTransaction(askPtr);

                    askPtr = askPtr.next;
                    removeOrder(askCancel);
                }

                else if (sellQuantity > buyQuantity)
                {
                    bidPtr.CurrentOrder.DecreaseQuantity(buyQuantity);
                    askPtr.CurrentOrder.DecreaseQuantity(buyQuantity);

                    result.addTransaction(bidPtr);

                    bidPtr = bidPtr.next;
                    removeOrder(bidCancel);
                }

                else 
                {
                    bidPtr.CurrentOrder.DecreaseQuantity(buyQuantity);
                    askPtr.CurrentOrder.DecreaseQuantity(sellQuantity);

                    result.addTransaction(askPtr);
                    result.addTransaction(bidPtr);

                    askPtr = askPtr.next;
                    bidPtr = bidPtr.next;

                    removeOrder(askCancel);
                    removeOrder(bidCancel);
                }

                    // result.addTransaction(askPtr);
            }
            
            return result;
        }

        private readonly Security _security;
    }
}