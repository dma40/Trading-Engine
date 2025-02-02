namespace TradingServer.Orders
{
    public record OrderRecord(long orderID, uint quantity, long price, 
    bool isBuySide, string username, int SecurityID, uint queuePosition);
}