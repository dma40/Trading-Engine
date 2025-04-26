using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;
using Trading;
using TradingServer.Orders;
using TradingServer.Rejects;

namespace TradingServer.Core 
{
    sealed class TradingServer: BackgroundService, ITradingServer 
    {
        private readonly ITextLogger _logger;
        private readonly IReadOnlyOrderbook _orderbook; 
        private readonly TradingServerConfiguration _tradingConfig;

        public PermissionLevel permissionLevel;

        public TradingServer(ITextLogger logger, IOptions<TradingServerConfiguration> config) 
        {
            _logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            _tradingConfig = config.Value ?? throw new ArgumentNullException("config cannot be null");
            _orderbook = OrderbookPermissions.createOrderbookFromConfig(_tradingConfig.TradingServerSettings.SecurityName, _tradingConfig.PermissionLevel);
            permissionLevel = config.Value.PermissionLevel;
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            _logger.LogInformation(nameof(TradingServer), "Starting Process");
            _logger.LogInformation(nameof(TradingServer), $"Security name: {_tradingConfig.TradingServerSettings.SecurityName}");
            _logger.LogInformation(nameof(TradingServer), $"Security ID: {_tradingConfig.TradingServerSettings.SecurityID}");
            
            while (!stoppingToken.IsCancellationRequested) 
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();
                cts.Dispose();
            }

            _logger.LogInformation(nameof(TradingServer), $"Ending process {nameof(TradingServer)}");
            return Task.CompletedTask;

            // For extra fun: add something that sends a email once the trading session concludes/trading server terminates
        }

        private Tuple<bool, Reject> checkIfOrderIsInvalid(OrderRequest request)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan onOpenDeadline = new TimeSpan(9, 28, 0);
            TimeSpan onCloseDeadline = new TimeSpan(15, 50, 0);

            RejectCreator creator = new RejectCreator(); 
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, 
                                                _tradingConfig.TradingServerSettings.SecurityID, 
                                                Order.StringToOrderType(request.Type));

            bool isInvalid = false;
            RejectionReason reason = RejectionReason.Unknown;

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

            else if (request.Operation == "Add" && _orderbook.containsOrder(request.Id))
            {
                isInvalid = true;
                reason = RejectionReason.ModifyWrongSide;
            }

            else if ((request.Operation == "Modify" || request.Operation == "Cancel") 
                    && !_orderbook.containsOrder(request.Id))
            {
                isInvalid = true;
                reason = RejectionReason.OrderNotFound;
            }

            else if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
            {
                isInvalid = true;
                reason = RejectionReason.InvalidOrUnknownArgument;
            }

            Reject reject = RejectCreator.GenerateRejection(orderCore, reason);

            Tuple<bool, Reject> result = new Tuple<bool, Reject>(isInvalid, reject);
            return result;
        }

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            if ((int) permissionLevel < 2)
            {
                throw new UnauthorizedAccessException("401 Permission Error");
            }

            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _tradingConfig.TradingServerSettings.SecurityID, Order.StringToOrderType(request.Type)); 
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");
            DateTime now = DateTime.Now;

            Tuple<bool, Reject> result = checkIfOrderIsInvalid(request);

            if (now.Hour >= 16)
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 403,
                    Message = "You cannot submit orders now, the exchange is closed.\n" + 
                    "Please try again when the market reopens at 9:30 AM"
                };
            }

            else if (result.Item1)
            {
                _logger.Error(nameof(TradingServer), RejectCreator.RejectReasonToString(result.Item2.reason));

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = RejectCreator.RejectReasonToString(result.Item2.reason)
                };
            }

            else if (request.Operation == "Add")
            {
                Order newOrder = modify.newOrder();
                _orderbook.addOrder(newOrder);
                
                
                _logger.LogInformation(nameof(TradingServer), $"Order {request.Id} added to {request.Side}" + 
                " side by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Cancel")
            {
                CancelOrder cancelOrder = modify.cancelOrder();
                _orderbook.removeOrder(cancelOrder);

                _logger.LogInformation(nameof(TradingServer), $"Removed order {request.Id}" + 
                " by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Modify")
            {
                _orderbook.modifyOrder(modify);
                
                _logger.LogInformation(nameof(TradingServer), $"Modified order {request.Id} in {request.Side}" +
                 " by {request.Username} at {DateTime.UtcNow}");
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