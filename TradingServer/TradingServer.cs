
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;
using Trading;
using TradingServer.Orders;
using TradingServer.Instrument;
using TradingServer.Rejects;

namespace TradingServer.Core 
{
    class TradingServer: BackgroundService, ITradingServer 
    {
        private readonly ITextLogger _logger;
        private readonly IMatchingOrderbook _orderbook;
        private readonly TradingServerConfiguration _tradingConfig;

        public TradingServer(ITextLogger logger, IOptions<TradingServerConfiguration> config) 
        {
            _logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            _tradingConfig = config.Value ?? throw new ArgumentNullException("config cannot be null");
            _orderbook = new FIFOrderbook(new Security(_tradingConfig.TradingServerSettings.SecurityName));
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
        }

        private Tuple<bool, Reject> checkIfOrderIsInvalid(OrderRequest request)
        {
            RejectCreator creator = new RejectCreator(); 
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _tradingConfig.TradingServerSettings.SecurityID, Order.StringToOrderType(request.Type));

            bool isInvalid = false;
            RejectionReason reason = RejectionReason.Unknown;

            if (string.IsNullOrWhiteSpace(request.Id.ToString()) 
                || string.IsNullOrWhiteSpace(request.Username) 
                || string.IsNullOrWhiteSpace(request.Type))
            {
                isInvalid = true;
                reason = RejectionReason.InvalidOrEmptyArgument;
            }

            else if (request.Operation == "Add" && _orderbook.containsOrder(request.Id))
            {
                isInvalid = true;
                reason = RejectionReason.ModifyWrongSide;
            }

            else if ((request.Operation == "Modify" || request.Operation == "Cancel") && _orderbook.containsOrder(request.Id))
            {
                isInvalid = true;
                reason = RejectionReason.OrderNotFound;
            }

            else if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
            {
                isInvalid = true;
                reason = RejectionReason.InvalidOrEmptyArgument;
            }

            Reject reject = RejectCreator.GenerateRejection(orderCore, reason);

            Tuple<bool, Reject> result = new Tuple<bool, Reject>(isInvalid, reject);
            return result;
        }

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            // maybe somewhere set the creation time to be at 9:30 in the morning if the result comes in at after hours
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _tradingConfig.TradingServerSettings.SecurityID, Order.StringToOrderType(request.Type)); // do this for now,
                                                                                                                                                        // this is the only existing order type after all
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");
            DateTime now = DateTime.Now;

            Tuple<bool, Reject> result = checkIfOrderIsInvalid(request);

            if (result.Item1)
            {
                _logger.Error(nameof(TradingServer), RejectCreator.RejectReasonToString(result.Item2.reason));
            }

            else if (now.Hour >= 16)
            {
                // return new OrderResponse
                // {
                //    Id = request.Id,
                //    Status = 403,
                //    Message = "You cannot submit orders now, the exchange is closed. Please try again when the market reopens"
                //};
            }

            else if (request.Operation == "Add")
            {
                Order newOrder = modify.newOrder();
                _orderbook.addOrder(newOrder);

                _logger.LogInformation(nameof(TradingServer), $"Order {request.Id} added to {request.Side} side by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Cancel")
            {
                CancelOrder cancelOrder = modify.cancelOrder();
                _orderbook.removeOrder(cancelOrder);

                _logger.LogInformation(nameof(TradingServer), $"Removed order {request.Id} by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Modify")
            {
                // here, maybe get the order, then get the id, and then put it back in the orderbook as a new order
                // and then try to match it

                _orderbook.modifyOrder(modify);

                //_logger.Debug(nameof(TradingServer), $"Ask side limits: " + askSideLimits);
                //_logger.Debug(nameof(TradingServer), $"Bid side limits" + bidSideLimits);
                
                _logger.LogInformation(nameof(TradingServer), $"Modified order {request.Id} in {request.Side} by {request.Username} at {DateTime.UtcNow}");
            }

            _logger.LogInformation(nameof(TradingServer), $"Processed {request.Id} from {request.Username} successfully");

            string askSideIds = "";
            string bidSideIds = "";

            string bidSideLimits = "";
            string askSideLimits = "";

            // do this for additional debugging

            foreach (var ask in _orderbook.getAskOrders())
            {
                askSideIds += $" {ask.CurrentOrder.OrderID} ";
            }

            foreach (var bid in _orderbook.getBidOrders())
            {
                bidSideIds += $" {bid.CurrentOrder.OrderID} ";
            }

            foreach (var ask in _orderbook.getAskLimits())
            {
                askSideLimits += $" {ask.Price} ";
            }

            foreach (var bid in _orderbook.getBidLimits())
            {
                bidSideLimits += $" {bid.Price} ";
            }

            _logger.Debug(nameof(TradingServer), $"Bid orders in the orderbook: " + bidSideIds);
            _logger.Debug(nameof(TradingServer), $"Ask orders in the orderbook: " + askSideIds);
            _logger.Debug(nameof(TradingServer), $"Ask side limits: " + askSideLimits);
            _logger.Debug(nameof(TradingServer), $"Bid side limits" + bidSideLimits);
            
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