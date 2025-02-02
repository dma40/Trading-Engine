using TradingServer.Orders;

namespace TradingServer.Rejects 
{
    public enum RejectionReason 
    {
        Unknown,
        OrderNotFound,
        ModifyWrongSide,
        OperationNotFound
    }
}