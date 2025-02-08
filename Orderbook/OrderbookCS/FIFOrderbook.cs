using TradingServer.Instrument;
using TradingServer.Orders;
namespace TradingServer.Orderbook
// We cannot inherit from Orderbook because there is 
// no limit in the regular orderbook
{
    public class FIFOrderbook : Orderbook, IMatchingOrderbook
    {
        public FIFOrderbook(Security instrument): base(instrument)
        {
            _instrument = instrument;
        }
    
        public MatchResult match()
        {
            MatchResult result = new MatchResult();
            List<OrderbookEntry> asks = getAskOrders();
            List<OrderbookEntry> bids = getBidOrders();
            //while (true)
            //{
            //
            //}
            return result;
            // after we're done match everything back into it limits; clear out all the limits first
            // Luckily the limits operate under the hood
            // What should be returned in the result?
            // Maybe return a OrderRecord of all matched orders
            // Pool all the bid and ask orders together, and then match them
            // find a way to assign things in a dictionary to limits
        }

        private readonly Security _instrument;
    }
    
}