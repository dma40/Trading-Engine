namespace TradingServer.Orders 
{
    public class BidLimitComparer: IComparer<Limit>
    {
        public static IComparer<Limit> comparer { get; } = new BidLimitComparer();
        public int Compare(Limit limit, Limit other)
        {
            if (limit.Price == other.Price)
                return 0;
            
            else if (limit.Price > other.Price)
                return -1;
            
            else 
                return 1;
        }
    }

    public class AskLimitComparer: IComparer<Limit>
    {
        public static IComparer<Limit> comparer { get; } = new AskLimitComparer();
        public int Compare(Limit limit, Limit other)
        {
            if (limit.Price == other.Price)
                return 0;
            
            else if (limit.Price > other.Price)
                return -1;
            
            else 
                return 1;   
        }
    }
}