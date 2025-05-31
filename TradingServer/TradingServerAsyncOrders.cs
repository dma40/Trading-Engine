using Microsoft.Extensions.Hosting;
using Trading;
using TradingServer.Orders;
using TradingServer.Services;
using Grpc.Core;

namespace TradingServer.Core
{
    internal sealed partial class TradingServer: BackgroundService, ITradingServer
    {
        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request, ServerCallContext context)
        {
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _security.id, Order.StringToOrderType(request.Type));
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");
            DateTime now = DateTime.Now;

            if (request.Operation == "Add")
            {
                bool error = false;

                try
                {
                    Order newOrder = modify.newOrder();
                    _engine.addOrder(newOrder);
                }

                catch (Exception exception)
                {
                    _logger.Error(nameof(TradingServer), exception);
                    error = true;
                }

                if (!error)
                {
                    _logger.LogInformation(nameof(TradingServer), $"Order {request.Id} added to {request.Side}" +
                    $" side by {request.Username} at {DateTime.UtcNow}");
                }
            }

            else if (request.Operation == "Cancel")
            {
                bool error = false;

                try
                {
                    CancelOrder cancelOrder = modify.cancelOrder();
                    _engine.removeOrder(cancelOrder);
                }

                catch (Exception exception)
                {
                    _logger.LogInformation(nameof(TradingServer), exception);
                    error = true;
                }

                if (!error)
                {
                    _logger.LogInformation(nameof(TradingServer), $"Removed order {request.Id}" +
                    $"by {request.Username} at {DateTime.UtcNow}");
                }
            }

            else if (request.Operation == "Modify")
            {
                bool error = false;

                try
                {
                    _engine.modifyOrder(modify);
                }

                catch (Exception exception)
                {
                    _logger.Error(nameof(TradingServer), exception);
                    error = true;
                }

                if (!error)
                {
                    _logger.LogInformation(nameof(TradingServer), $"Modified order {request.Id} in {request.Side}" +
                    $" by {request.Username} at {DateTime.UtcNow}");
                }
            }

            await Task.Delay(200);

            return new OrderResponse
            {
                Id = request.Id,
                Status = 200,
                Message = "Operation executed successfully!"
            };
        }
    }
}