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
        TrailingStop,

        MarketOnClose,
        LimitOnClose,
        MarketOnOpen,
        LimitOnOpen
    }
}