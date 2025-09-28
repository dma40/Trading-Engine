using TradingServer.Core.Configuration;
using TradingServer.OrderbookCS;
using TradingServer.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingServer.Rejects;
using TradingServer.Instrument;
using Trading;
using Grpc.Core;
using TradingServer.Services;
using Mysqlx.Cursor;

namespace TradingServer.Core
{
    internal sealed partial class TradingServer : BackgroundService, ITradingServer
    {
        private readonly ITextLogger _logger;
        private readonly Security _security;
        private readonly TradingServerConfiguration _tradingConfig;
        private readonly OrderValidator _validator;

        private readonly IMatchingEngine _engine;

        public TradingServer(ITextLogger logger, IOptions<TradingServerConfiguration> config)
        {
            _logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            _tradingConfig = config.Value ?? throw new ArgumentNullException("config cannot be null");

            string name = _tradingConfig?.TradingServerSettings?.SecurityName ?? throw new ArgumentNullException();
            string id = _tradingConfig?.TradingServerSettings?.SecurityID ?? throw new ArgumentNullException();

            _security = new Security(name, id);
            _engine = new TradingEngine(_security);
            _validator = OrderValidator.Instance;
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(nameof(TradingServer), "Starting Process");
            _logger.LogInformation(nameof(TradingServer), $"Security name: {_security.name}");
            _logger.LogInformation(nameof(TradingServer), $"Security ID: {_security.id}");

            while (!stoppingToken.IsCancellationRequested)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();
                cts.Dispose();
            }

            _logger.LogInformation(nameof(TradingServer), $"Ending process {nameof(TradingServer)}");
            return Task.CompletedTask;
        }

        public async Task processIncomingRequest(OrderRequest request, ServerCallContext context)
        {
            await submitOrder(request, context);
        }
    }
}