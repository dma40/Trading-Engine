
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;
using Trading;
using TradingServer.Orders;

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
            _orderbook = new FIFOrderbook(new Instrument.Security("AAPL"));
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            _logger.LogInformation(nameof(TradingServer), "Starting Process");
            
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
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, request.Id);
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");

            if (string.IsNullOrEmpty(request.Id.ToString()) 
                || string.IsNullOrEmpty(request.Username.ToString())
                || string.IsNullOrEmpty(request.Operation.ToString())
                || string.IsNullOrEmpty(request.Side.ToString())
                )
            {
                _logger.LogInformation(nameof(TradingServer), $"Rejected order with invalid arguments at {DateTime.Now}");

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "Error: you have put in null or empty values for arguments"
                };

                throw new InvalidOperationException("Cannot have null or empty arguments");
            }

            else if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
            {
                _logger.LogInformation(nameof(TradingServer), $"Rejected request attempting to add to a invalid side at {DateTime.Now}");

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "Error: you have tried to put a order on a invalid side"
                };
            }

            else if (request.Operation == "Add")
            {
                if (_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.LogInformation(nameof(TradingServer), $"Rejected request to add order that exists in the orderbook at {DateTime.Now}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: this order already exists within the orderbook"
                    };
                }

                Order newOrder = modify.newOrder();
                _orderbook.addOrder(newOrder);
            }

            else if (request.Operation == "Cancel")
            {
                if (!_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.LogInformation(nameof(TradingServer), $"Rejected a request to cancel a order not in the orderbook at {DateTime.Now}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you cannot cancel a order that is not currently in the orderbook"
                    };
                }

                CancelOrder cancelOrder = modify.cancelOrder();
                _orderbook.removeOrder(cancelOrder);
            }

            else if (request.Operation == "Modify")
            {
                if (!_orderbook.containsOrder(modify.OrderID))
                {
                    _logger.LogInformation(nameof(TradingServer), $"Rejected a request to modify a order that does not exist at {DateTime.Now}");

                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you cannot cancel modify a order that is not currently in the orderbook"
                    };
                    throw new InvalidOperationException("Cannot modify a non-existent order");
                }

                _orderbook.modifyOrder(modify);
            }

            else
            {
                _logger.LogInformation(nameof(TradingServer), $"Rejected request with unknown error at {DateTime.Now}");

                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "An unkown error occurred"
                };
            }

            _logger.LogInformation(nameof(TradingServer), $"Processed {request.Id} successfully");

            if (_orderbook.canMatch())
            {
                _logger.LogInformation(nameof(TradingServer), "Matching orders...");
                _orderbook.match();
                _logger.LogInformation(nameof(TradingServer), $"Order match executed at {DateTime.Now}");
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