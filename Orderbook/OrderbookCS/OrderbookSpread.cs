namespace TradingServer.OrderbookCS
{
    public class OrderbookSpread
    {
        public OrderbookSpread(long? Bid, long ?Ask)
        {
            bid = Bid;
            ask = Ask;
        }

        public long? bid { get; private set; }
        public long? ask { get; private set; }
        public long? spread 
        {
            get
            {
                if (bid.HasValue && ask.HasValue)
                    return (ask.Value - bid.Value);
                
                else
                    return null;
            }
        }
    }
}