namespace TradingServer.Orders
{
    public enum OrderTypes
    {
        Limit,
        GoodForDay, 
        FillOrKill, 
        FillAndKill, 
        GoodTillCancel, 
        Market, 
        PostOnly,
        

        MarketOnClose,
        LimitOnClose,
        MarketOnOpen,
        LimitOnOpen,
    }
}