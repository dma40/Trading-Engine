using TradingServer.Tests;

using TradingServer.Core;

Console.WriteLine("Starting trading server...\n");

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

_ = Task.Run(() => processInputs());

await engine.StartAsync().ConfigureAwait(false);
OrderbookUnitTests.runTests();


static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




