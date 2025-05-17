namespace TradingServer.Core
{
    public enum Actions
    {
        ContainsOrder,
        GetCount,
        GetSpread, // Level 1 data
        GetAskOrders,
        GetBidOrders, // Level 2 data
        AddOrder,
        ModifyOrder,
        RemoveOrder // Level 3 data
    }
}