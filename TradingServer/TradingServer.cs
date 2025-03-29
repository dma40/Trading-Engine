
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

using TradingServer.Logging;
using TradingServer.OrderbookCS;
using TradingServer.Handlers;

namespace TradingServer.Core 
{
    class TradingServer: BackgroundService, ITradingServer 
    {

        private readonly ITextLogger _logger;
        private readonly IMatchingOrderbook _orderbook;
        private readonly TradingServerConfiguration _tradingConfig;
        // private Orderbook orders;

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
            // _logger.LogInformation($"Ending process {nameof(TradingServer)}");
            return Task.CompletedTask;
        }

        // public async void SubmitOrder(OrderRequest request, ServerCallContext call)
        // {
        // now that the OrderRequest is being recieved, find a way to pipe it onto the orderbook!
        // make sure that the OrderRequest is ACTUALLY being recieved by the hosted TradingServer service.
        // }
    }
}