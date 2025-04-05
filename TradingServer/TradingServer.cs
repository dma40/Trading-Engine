
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
            // do this for now
            _orderbook = new FIFOrderbook(new Instrument.Security("AAPL"));
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            _logger.LogInformation(nameof(TradingServer), "Starting Process");
            while (!stoppingToken.IsCancellationRequested) 
            {
                // CancellationTokenSource cts = new CancellationTokenSource();
                //cts.Cancel();
                //cts.Dispose();
            }
            _logger.LogInformation(nameof(TradingServer), $"Ending process {nameof(TradingServer)}");
            return Task.CompletedTask;
        }

        public async Task<OrderResponse> ProcessOrderAsync(OrderRequest request)
        {
            // Next steps: 
            // - Find out how to add a order to our orderbook
            // Then, add support for other operations (such as Buy, Sell, Modify and Cancel)

            // maybe also log the fact that this order was placed
            // First, check that none of the requisite information is wrong/missing

            // make sure so that the id, price, and quantity are all integers

            /*
            Precondition: everything is of the right type; for example, username is a string
            */
            IOrderCore orderCore = new OrderCore(request.Id, request.Username, request.Id);
            ModifyOrder modify = new ModifyOrder(orderCore, request.Price, request.Quantity, request.Side == "Bid");

            if (string.IsNullOrEmpty(request.Id.ToString()) 
                || string.IsNullOrEmpty(request.Username.ToString())
                || string.IsNullOrEmpty(request.Operation.ToString())
                || string.IsNullOrEmpty(request.Side.ToString())
                )
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "You have put in null or empty strings for arguments"
                };
                // or throw a new InvalidOperationException
            }

            else if (request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask")
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "You have tried to put a order on a invalid side"
                };
            }

            else if (request.Operation == "Add")
            {
                /*
                This time, we need to make sure that the order we're trying to add
                DOES NOT exist in the orderbook.
                */
                if (_orderbook.containsOrder(modify.OrderID))
                {
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
                /* 
                Make sure that our program knows what to do if we try to 
                modify orders that don't exist in the orderbook
                */
                if (!_orderbook.containsOrder(modify.OrderID))
                {
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
                /*
                Make sure, again, to test that the desired order actually exists!
                */
                if (!_orderbook.containsOrder(modify.OrderID))
                {
                    return new OrderResponse
                    {
                        Id = request.Id,
                        Status = 500,
                        Message = "Error: you cannot cancel modify a order that is not currently in the orderbook"
                    };
                }

                _orderbook.modifyOrder(modify);
            }

            await Task.Delay(200); 

            return new OrderResponse
            {
                Id = request.Id,
                Status = 200,
                Message = "Order placed successfully!"
            };

            // Console.WriteLine($"{request.Id}");
        }
    }
}