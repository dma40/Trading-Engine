using TradingServer.Tests;
using TradingServer.Core;
using Microsoft.AspNetCore.Components.Forms;

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

_ = Task.Run(() => processInputs());

Console.WriteLine("Run orderbook tests? y/n");
string input = Console.ReadLine() ?? "n";

if (input == "y")
    OrderbookUnitTests.runTests();

Console.WriteLine("Starting trading server...\n");
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




