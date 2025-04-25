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

            else if (reject == RejectionReason.EmptyOrNullArgument)
            {
                return "EmptyOrNullArgument";
            }

            else if (reject == RejectionReason.ModifyWrongSide)
            {
                return "ModifyWrongSide";
            }

            else if (reject == RejectionReason.OperationNotFound)
            {
                return "OrderNotFound";
            }

            else if (reject == RejectionReason.InvalidOrUnknownArgument)
            {
                return "InvalidOrUnknownArgument";
            }

            else if (reject == RejectionReason.SubmittedAfterDeadline)
            {
                return "SubmittedAfterDeadline";
            }

            else 
            {
                return "Unknown";
            }
        }
    }
}