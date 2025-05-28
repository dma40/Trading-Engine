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
                return "500: Unknown";
            }

            else if (reject == RejectionReason.EmptyOrNullArgument)
            {
                return "400: EmptyOrNullArgument";
            }

            else if (reject == RejectionReason.ModifyWrongSide)
            {
                return "400: ModifyWrongSide";
            }

            else if (reject == RejectionReason.OperationNotFound)
            {
                return "404: OrderNotFound";
            }

            else if (reject == RejectionReason.InvalidOrUnknownArgument)
            {
                return "400: InvalidOrUnknownArgument";
            }

            else if (reject == RejectionReason.SubmittedAfterDeadline)
            {
                return "403: SubmittedAfterDeadline";
            }

            else if (reject == RejectionReason.InsufficientPermissionError)
            {
                return "401: InsufficientPermissionError";
            }

            else
            {
                return "500: Unknown";
            }
        }
    }
}