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
using ZstdSharp.Unsafe;
using System.Threading.Tasks;

namespace TradingServer.Core
{
    internal sealed partial class TradingServer : BackgroundService, ITradingServer
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

        public async Task processIncomingRequest(OrderRequest request, ServerCallContext context)
        {
            // provide support for the level 1, 2 operations here 
            await submitOrder(request, context);
        }

        
    }
}