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
                    // process the ask side
                 }

                 else 
                 {
                    // process the bid side
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
                    // process the ask side
                }

                else 
                {
                    // process the bid side
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

            else 
            {
                // treat as a standard GTC or GFD
            }
            return result;
        }
    

        private readonly Security _security;
    }
}