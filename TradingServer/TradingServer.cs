using TradingServer.Core.Configuration;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using TradingServer.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Rejects;
using TradingServer.Instrument;
using Trading;
using Grpc.Core;
using TradingServer.Services;
using Google.Protobuf.Reflection;

namespace TradingServer.Core 
{
    internal sealed class TradingServer: BackgroundService, ITradingServer 
    {
        private readonly ITextLogger _logger;
        private readonly Security _security; 
        private readonly TradingServerConfiguration _tradingConfig;
        public readonly TradingEngine _engine;

        public PermissionLevel permissionLevel;

        public TradingServer(ITextLogger logger, IOptions<TradingServerConfiguration> config) 
        {
            _logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            _tradingConfig = config.Value ?? throw new ArgumentNullException("config cannot be null");

            _security = new Security(_tradingConfig?.TradingServerSettings?.SecurityName ?? throw new ArgumentNullException());
            _engine = new TradingEngine(_security);
            permissionLevel = config.Value.PermissionLevel;
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            _logger.LogInformation(nameof(TradingServer), "Starting Process");
            _logger.LogInformation(nameof(TradingServer), $"Security name: {_tradingConfig.TradingServerSettings?.SecurityName}");
            _logger.LogInformation(nameof(TradingServer), $"Security ID: {_tradingConfig.TradingServerSettings?.SecurityID}");
            
            while (!stoppingToken.IsCancellationRequested) 
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();
                cts.Dispose();
            }

            _logger.LogInformation(nameof(TradingServer), $"Ending process {nameof(TradingServer)}");
            return Task.CompletedTask;
        }

        public void checkIfOrderIsInvalid(OrderRequest request)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan onOpenDeadline = new TimeSpan(9, 28, 0);
            TimeSpan onCloseDeadline = new TimeSpan(15, 50, 0);
            TimeSpan closed = new TimeSpan(16, 0, 0);

            IOrderCore orderCore = new OrderCore(request.Id, request.Username, 
                                                _tradingConfig?.TradingServerSettings?.SecurityID ?? throw new ArgumentNullException("Securit ID cannot be null"), 
                                                Order.StringToOrderType(request.Type));

            bool isInvalid = false;
            RejectionReason reason = RejectionReason.Unknown;

            if ((int) permissionLevel < 2)
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
        }

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request, ServerCallContext context)
        {
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _tradingConfig?.TradingServerSettings?.SecurityID ?? throw new ArgumentNullException("Security ID cannot be null"), Order.StringToOrderType(request.Type)); 
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");
            DateTime now = DateTime.Now;

            if (request.Operation == "Add")
            {
                bool exception = false;

                try
                {
                    Order newOrder = modify.newOrder();
                    _engine.addOrder(newOrder);
                }

                catch (InvalidOperationException error)
                {
                    _logger.Error(nameof(TradingServer), error.Message + $" {DateTime.Now}");
                    exception = true;
                }

                if (!exception)
                    _logger.LogInformation(nameof(TradingServer), $"Order {request.Id} added to {request.Side}" +
                    $" side by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Cancel")
            {
                bool error = false;

                try
                {
                    CancelOrder cancelOrder = modify.cancelOrder();
                    _engine.removeOrder(cancelOrder);
                }

                catch (InvalidOperationException exception)
                {
                    _logger.LogInformation(nameof(TradingServer), exception.Message + $" {DateTime.Now}");
                    error = true;
                }

                if (!error)
                    _logger.LogInformation(nameof(TradingServer), $"Removed order {request.Id}" +
                    $"by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Modify")
            {
                bool error = false;

                try
                {
                    _engine.modifyOrder(modify);
                }

                catch (InvalidOperationException exception)
                {
                    _logger.Error(nameof(TradingServer), exception.Message + $"{DateTime.Now}");
                    error = true;
                }

                if (!error)
                    _logger.LogInformation(nameof(TradingServer), $"Modified order {request.Id} in {request.Side}" +
                    $" by {request.Username} at {DateTime.UtcNow}");
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