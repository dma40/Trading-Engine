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

        public MatchResult match() // maybe have a order as a argument here, see if it can be matched
        // whether or not it is added later is going to be handled seperately
        // we only let orders match with things that have a lower price than it (or higher depending on whether it's a bid or ask order)
        // then based on its order type (Market, FillOrKill, IntermediateOrCancel, etc) we either add or don't add it to the orderbook
        {
            // we also need to know what side the incoming order is in 
            if (getAskLimits().Count == 0 || getBidLimits().Count == 0)
            {
                throw new InvalidOperationException("Orders cannot be matched because either the buy side or the sell side of the orderbook is empty");
            }
            
            MatchResult result = new MatchResult();
            // also this is mostly wrong, we don't match orders like that
            // study the matching algorithm more, how are the market orders matched?

            // add seperate processing for FillOrKill, IntermediateOrCancel, Market, AllOrNone

            // also do the thing where we match the GoodForDay, GoodTillCancel order against the thing
            return result;
        }
    

        private readonly Security _security;
    }
}