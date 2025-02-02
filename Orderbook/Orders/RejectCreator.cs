using System;

using TradingServer.Orders;

namespace TradingServer.Rejects 
{
    public sealed class RejectCreator 
    {
        public static Reject GenerateRejection(IOrderCore rejected, RejectionReason reason) 
        {
            return new Reject(rejected, reason);
        }
    }
}