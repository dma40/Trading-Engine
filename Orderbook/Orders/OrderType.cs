namespace TradingServer.Orders
{
    public enum OrderTypes
    {
        GoodForDay, 
        FillOrKill, 
        FillAndKill, 
        GoodTillCancel, 
        Market, 
        PostOnly
    }
}