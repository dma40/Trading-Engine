
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;
using Trading;
using TradingServer.Orders;
using Microsoft.AspNetCore.Hosting.Server.Features;

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

            // This will probably require a lot of if statements, there are a lot of things that can 
            // go wrong here.
            if (string.IsNullOrEmpty(request.Id.ToString()))
            {
                return new OrderResponse
                {
                    Id = request.Id,
                    Status = 500,
                    Message = "You have placed a invalid order"
                };
            }

            // _orderbook.addOrder();
            await Task.Delay(200); 
            // add exception handling - what if something is wrong?

            return new OrderResponse
            {
                Id = request.Id,
                Status = 200,
                Message = "Order placed successfully!"
            };
        }
    }
}