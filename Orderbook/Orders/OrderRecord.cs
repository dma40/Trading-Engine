namespace TradingServer.Orders
{
    public record OrderRecord(long orderID, uint initialQuantity, uint finalQuantity, long price, long endPrice,
        bool isBuySide, string username, string SecurityID, uint queuePositionInitial, uint queuePositionFinal);
}