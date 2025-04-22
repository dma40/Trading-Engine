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

        Stop,
        StopLimit,
        TrailingStop,

        MarketOnClose,
        LimitOnClose,
        MarketOnOpen,
        LimitOnOpen
    }
}