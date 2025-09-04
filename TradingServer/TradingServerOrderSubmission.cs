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
          
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _security.id, Order.StringToOrderType(request.Type));

            RejectionReason reason = _validator.Check(request);

            if (reason != RejectionReason.None)
            {
                _logger.Error(nameof(TradingServer), RejectCreator.RejectReasonToString(reason));
            }

            else
            {
                OrderResponse response = await ProcessOrderAsync(request, context);
                _logger.LogInformation(nameof(TradingServer), $"ID: {response.Id} Status: {response.Status} Message: {response.Message}");
            }
        }
    }
}