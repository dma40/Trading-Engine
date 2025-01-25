using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingServer.Core.Configuration;

namespace TradingServer.Core {

    class TradingServer: BackgroundService, ITradingServer {

        private readonly ILogger<TradingServer> _logger;
        private readonly TradingServerConfiguration _tradingConfig;

        public TradingServer(ILogger<TradingServer> logger, IOptions<TradingServerConfiguration> config) {
            _logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            _tradingConfig = config.Value ?? throw new ArgumentNullException("config cannot be null");
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation($"Starting process {nameof(TradingServer)}");
            while (!stoppingToken.IsCancellationRequested) {
                // CancellationTokenSource cts = new CancellationTokenSource();
                //cts.Cancel();
                //cts.Dispose();
            }
            // _logger.LogInformation($"Ending process {nameof(TradingServer)}");
        return Task.CompletedTask;
        }
    }
}