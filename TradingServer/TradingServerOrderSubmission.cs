using Microsoft.Extensions.Hosting;
using Trading;
using TradingServer.Orders;
using TradingServer.Services;
using TradingServer.Rejects;
using Grpc.Core;

namespace TradingServer.Core
{
    internal sealed partial class TradingServer : BackgroundService, ITradingServer
    {
        public async Task submitOrder(OrderRequest request, ServerCallContext context)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan onOpenDeadline = new TimeSpan(9, 28, 0);
            TimeSpan onCloseDeadline = new TimeSpan(15, 50, 0);
            TimeSpan closed = new TimeSpan(16, 0, 0);

            IOrderCore orderCore = new OrderCore(request.Id, request.Username,
                                                _tradingConfig?.TradingServerSettings?.SecurityID
                                                ?? throw new ArgumentNullException("Securit ID cannot be null"),
                                                Order.StringToOrderType(request.Type));

            bool isInvalid = false;
            RejectionReason reason = RejectionReason.Unknown;

            if ((int)permissionLevel < 2)
            {
                isInvalid = true;
                reason = RejectionReason.InsufficientPermissionError;
            }

            if (Order.StringToOrderType(request.Type) == OrderTypes.MarketOnClose ||
                Order.StringToOrderType(request.Type) == OrderTypes.LimitOnClose && now >= onCloseDeadline)
            {
                isInvalid = true;
                reason = RejectionReason.SubmittedAfterDeadline;
            }

            else if (Order.StringToOrderType(request.Type) == OrderTypes.MarketOnOpen ||
                    Order.StringToOrderType(request.Type) == OrderTypes.LimitOnOpen && now >= onOpenDeadline)
            {
                isInvalid = true;
                reason = RejectionReason.SubmittedAfterDeadline;
            }

            else if (now >= closed)
            {
                isInvalid = true;
                reason = RejectionReason.SubmittedAfterDeadline;
            }

            else if (Order.StringToOrderType(request.Type) == OrderTypes.Market &&
                (!string.IsNullOrWhiteSpace(request.Price.ToString())
                 || !string.IsNullOrWhiteSpace(request.Operation)))
            {
                isInvalid = true;
                reason = RejectionReason.EmptyOrNullArgument;
            }

            else if ((Order.StringToOrderType(request.Type) == OrderTypes.FillAndKill
                || (Order.StringToOrderType(request.Type) == OrderTypes.FillOrKill))
                && (!string.IsNullOrEmpty(request.Operation)))
            {
                isInvalid = true;
                reason = RejectionReason.OperationNotFound;
            }

            else if (string.IsNullOrWhiteSpace(request.Id.ToString())
                || string.IsNullOrWhiteSpace(request.Username))
            {
                isInvalid = true;
                reason = RejectionReason.EmptyOrNullArgument;
            }

            else if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
            {
                isInvalid = true;
                reason = RejectionReason.InvalidOrUnknownArgument;
            }

            Reject reject = RejectCreator.GenerateRejection(orderCore, reason);

            if (isInvalid)
            {
                _logger.Error(nameof(TradingServer), RejectCreator.RejectReasonToString(reject.reason));
            }

            else
            {
                OrderResponse response = await ProcessOrderAsync(request, context);
                _logger.LogInformation(nameof(TradingServer), $"ID: {response.Id} Status: {response.Status} Message: {response.Message}");
            }
        }
    }
}