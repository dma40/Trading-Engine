namespace TradingServer.Orders
{
    public enum OrderTypes
    {
        GoodForDay, // remove at end of day
        FillOrKill, // either fill the entire order - if it cannot be filled we delete it 
        FillAndKill, // can be partially filled but the rest of the order will be canceled
        GoodTillCancel, // we try to match and is cancelled when 90 days elapse since its creation - this is handled by brokers, but we'll have it here anyways.
        Market // try to fill at the best price(s) available
        // PostOnly
    }
}