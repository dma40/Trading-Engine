namespace TradingServer.Orders
{
    public enum OrderTypes
    {
        GoodForDay, 
        FillOrKill, 
        FillAndKill, 
        GoodTillCancel, 
        Market, 
        PostOnly,

        StopMarket,
        StopLimit,
        TrailingStopMarket,
        TrailingStopLimit,

        MarketOnClose,
        LimitOnClose,
        MarketOnOpen,
        LimitOnOpen
    }
}