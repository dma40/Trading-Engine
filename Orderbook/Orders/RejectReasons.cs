namespace TradingServer.Rejects 
{
    public enum RejectionReason 
    {
        Unknown,
        OrderNotFound,
        ModifyWrongSide,
        OperationNotFound,
        InvalidOrUnknownArgument,
        EmptyOrNullArgument,
        SubmittedAfterDeadline,
        InsufficientPermissionError
    }
}