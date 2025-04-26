using TradingServer.Tests;
using TradingServer.Core;

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

_ = Task.Run(() => processInputs());

OrderbookUnitTests.runTests();

Console.WriteLine("Starting trading server...\n");
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




