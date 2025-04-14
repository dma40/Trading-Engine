
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;
using Trading;
using TradingServer.Orders;
using TradingServer.Instrument;

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

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            // maybe somewhere set the creation time to be at 9:30 in the morning if the result comes in at after hours
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, _tradingConfig.TradingServerSettings.SecurityID, Order.StringToOrderType(request.Type)); // do this for now,
                                                                                                                                                        // this is the only existing order type after all
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");
            DateTime now = DateTime.Now;

            // TODO: add a seperate OrderResponse that details what to do if the market is closed right now

            if (string.IsNullOrEmpty(request.Id.ToString()) 
                || string.IsNullOrEmpty(request.Username.ToString())
                || string.IsNullOrEmpty(request.Operation.ToString())
                )
            {
                _logger.Error(nameof(TradingServer), $"Rejected order with invalid arguments submitted by {request.Username} at {DateTime.UtcNow}");

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "Error: you have put in null or empty values for arguments"
                };
            }

            else if (now.Hour >= 16)
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 403,
                    Message = "You cannot submit orders now, the exchange is closed. Please try again when the market reopens"
                };
            }

            else if (request.Operation == "Add")
            {
                if (_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.Error(nameof(TradingServer), $"Rejected request by {request.Username} to add order that exists in the orderbook at {DateTime.UtcNow}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: this order already exists within the orderbook"
                    };
                }

                // maybe add some try/catch blocks to test for various errors

                if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
                {
                    _logger.Error(nameof(TradingServer), $"Rejected request from {request.Username} attempting to add to a invalid side at {DateTime.UtcNow}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you have tried to put a order on a invalid side"
                    };
            }

                Order newOrder = modify.newOrder();
                _orderbook.addOrder(newOrder);

                _logger.LogInformation(nameof(TradingServer), $"Order {request.Id} added to {request.Side} side by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Cancel")
            {
                // no match occurs when we cancel a order
                if (!_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.Error(nameof(TradingServer), $"Rejected a request to cancel a order not in the orderbook from {request.Username} at {DateTime.UtcNow}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you cannot cancel a order that is not currently in the orderbook"
                    };
                }

                CancelOrder cancelOrder = modify.cancelOrder();
                _orderbook.removeOrder(cancelOrder);

                _logger.LogInformation(nameof(TradingServer), $"Removed order {request.Id} by {request.Username} at {DateTime.UtcNow}");
            }

            else if (request.Operation == "Modify")
            {
                // here, maybe get the order, then get the id, and then put it back in the orderbook as a new order
                // and then try to match it
                if (!_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.Error(nameof(TradingServer), $"Rejected a request to modify a order that does not exist from {request.Username} at {DateTime.UtcNow}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you cannot cancel modify a order that is not currently in the orderbook"
                    };
                }

                _orderbook.modifyOrder(modify);

                //_logger.Debug(nameof(TradingServer), $"Ask side limits: " + askSideLimits);
                //_logger.Debug(nameof(TradingServer), $"Bid side limits" + bidSideLimits);
                
                _logger.LogInformation(nameof(TradingServer), $"Modified order {request.Id} in {request.Side} by {request.Username} at {DateTime.UtcNow}");
            }

            else
            {
                _logger.Error(nameof(TradingServer), $"Rejected request with unknown error from {request.Username} at {DateTime.UtcNow}");

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "An unkown error occurred"
                };
            }

            _logger.LogInformation(nameof(TradingServer), $"Processed {request.Id} from {request.Username} successfully");
            /*
            if (_orderbook.canMatch())
            {
                _logger.LogInformation(nameof(TradingServer), $"Orders can now be matched in this orderbook. Order match started at {DateTime.UtcNow}");
                _orderbook.match();
                _logger.LogInformation(nameof(TradingServer), $"Order match executed at {DateTime.UtcNow}");
            }
            */

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