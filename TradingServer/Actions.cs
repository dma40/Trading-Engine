namespace TradingServer.Core
{
    public enum Actions
    {
        // Level 1 Data
        ContainsOrder,
        GetCount,
        GetSpread,

        // Level 2 Data
        GetAskOrders,
        GetBidOrders,

        // Level 3 Data
        AddOrder,
        ModifyOrder,
        RemoveOrder
    }
}