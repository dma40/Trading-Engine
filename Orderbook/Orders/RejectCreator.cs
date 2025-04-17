
using TradingServer.Orders;

namespace TradingServer.Rejects 
{
    public sealed class RejectCreator 
    {
        public static Reject GenerateRejection(IOrderCore rejected, RejectionReason reason) 
        {
            return new Reject(rejected, reason);
        }

        public static string RejectReasonToString(RejectionReason reject)
        {
            if (reject == RejectionReason.Unknown)
            {
                return "Unknown";
            }

            else if (reject == RejectionReason.InvalidOrEmptyArgument)
            {
                return "InvalidOrEmptyArgument";
            }

            else if (reject == RejectionReason.ModifyWrongSide)
            {
                return "ModifyWrongSide";
            }

            else if (reject == RejectionReason.OperationNotFound)
            {
                return "OrderNotFound";
            }

            else 
            {
                return "OrderNotFound";
            }
        }
    }
}