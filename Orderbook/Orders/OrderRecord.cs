namespace TradingServer.Orders
{
    // This type records the:
    // - ID of the order
    // - Change in quantity
    // - Price of the security (here, limit level) of the security being traded
    // - Whether the order executed (and subsequently deleted) was a buy or a sell side order
    // - user who submitted the order that was matched
    // - Position of that order within the limit
public record OrderRecord(long orderID, uint initialQuantity, uint finalQuantity, long price, long endPrice,
    bool isBuySide, string username, string SecurityID, uint queuePositionInitial, uint queuePositionFinal);
}