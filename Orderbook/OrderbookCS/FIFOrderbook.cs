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

        public bool canMatch(Order order)
        {
            return false; // once again we need the order's types...
        }

        // either fix up this matching engine first or do something else first

        public MatchResult match(Order order) // have a order as a argument here, see if it can be matched
        // whether or not it is added later is going to be handled seperately
        // we only let orders match with things that have a lower price than it (or higher depending on whether it's a bid or ask order)
        // then based on its order type (Market, FillOrKill, IntermediateOrCancel, etc) we either add or don't add it to the orderbook
        {
            // we also need to know what side the incoming order is in 
            
            MatchResult result = new MatchResult();

            if (order.OrderType == OrderTypes.FillOrKill)
            {
                /*
                We're going to need to:

                1: get all orders with price less than (or greater if the case need be) our order
                2: if there aren't enough shares placed then we cancel the whole thing
                3: go through and try to match this order with all of those orders
                 */
                 if (order.isBuySide)
                 {
                    // process the bid side
                 }

                 else 
                 {
                    // process the ask side
                 }
            }

            else if (order.OrderType == OrderTypes.FillAndKill)
            {
                /*
                Do the same thing except for the other one we're going to cancel only what remains
                so we'll still go through the whole order, only matching if we have orders at the desired price
                 */
                if (order.isBuySide)
                {
                    // process the bid side
                }

                else 
                {
                    // process the ask side
                }
            }

            else if (order.OrderType == OrderTypes.Market)
            {
                /*
                What we'll do is we'll add and not add the orer based off of the order's type
                */
                if (order.isBuySide)
                {
                    // process the bid side
                    // find all bid orders in sequential order
                }

                else 
                {
                    // process the ask side
                }
            }

            else // the only other cases are GoodTillCancel, GoodForDay which don't require special handling 
            {
                // treat as a standard GTC or GFD
            }
            // we need to get all things with a price less than the desired price, so that they can be matched
            // also this is mostly wrong, we don't match orders like that
            // study the matching algorithm more, how are the market orders matched?

            // add seperate processing for FillOrKill, IntermediateOrCancel, Market, AllOrNone

            // also do the thing where we match the GoodForDay, GoodTillCancel order against the thing
            return result;
        }
    

        private readonly Security _security;
    }
}