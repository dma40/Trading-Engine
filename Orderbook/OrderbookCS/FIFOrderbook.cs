using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    // maybe add support for FillOrKill, GoodForDay and IntermediateOrCancel
    // Aha! Maybe handle seperately the FillOrKill, GoodForDay and IntermediateOrCancels differently
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
            
            MatchResult result = new MatchResult();

            Limit min = getAskLimits().Min; // minimum nonempty limit
            Limit max = getBidLimits().Max; // maximum available nonempty limit

            while (min.head != null && max.head != null)
            {
                OrderbookEntry ask = min.head;
                OrderbookEntry bid = max.head;

                IOrderCore buyOrderCore = new OrderCore(bid.CurrentOrder.OrderID, bid.CurrentOrder.Username, bid.CurrentOrder.SecurityID);
                IOrderCore askOrderCore = new OrderCore(ask.CurrentOrder.OrderID, ask.CurrentOrder.Username, ask.CurrentOrder.SecurityID);
                CancelOrder bidCancel = new CancelOrder(buyOrderCore);
                CancelOrder askCancel = new CancelOrder(askOrderCore);

                uint buyQuantity = bid.CurrentOrder.CurrentQuantity;
                uint sellQuantity = ask.CurrentOrder.CurrentQuantity;

                if (buyQuantity > sellQuantity)
                {
                    bid.CurrentOrder.DecreaseQuantity(sellQuantity);
                    ask.CurrentOrder.DecreaseQuantity(sellQuantity);

                    result.addTransaction(ask);

                    removeOrder(askCancel);
                }

                else if (sellQuantity > buyQuantity)
                {
                    bid.CurrentOrder.DecreaseQuantity(buyQuantity);
                    ask.CurrentOrder.DecreaseQuantity(buyQuantity);

                    result.addTransaction(bid);

                    removeOrder(bidCancel);
                }

                else 
                {
                    bid.CurrentOrder.DecreaseQuantity(buyQuantity);
                    ask.CurrentOrder.DecreaseQuantity(sellQuantity);

                    result.addTransaction(ask);
                    result.addTransaction(bid);

                    removeOrder(askCancel);
                    removeOrder(bidCancel);
                }
            }
            
            return result;
        }

        private readonly Security _security;
    }
}