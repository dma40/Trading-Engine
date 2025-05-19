using TradingServer.Tests;
using TradingServer.Core;

Console.WriteLine("Starting trading server...\n");

_ = Task.Run(() => processInputs());
using var engine = TradingHostBuilder.BuildTradingServer();
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




