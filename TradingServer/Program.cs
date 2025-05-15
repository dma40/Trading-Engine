using TradingServer.Tests;
using TradingServer.Core;

Console.WriteLine(DateTime.Now.Hour > 9.5 && DateTime.Now.Hour < 16);

/*
Console.WriteLine("Run orderbook tests? (y/n)");
string input = Console.ReadLine() ?? "n";

if (input == "y")
    OrderbookUnitTests.runTests();
*/

Console.WriteLine("Starting trading server...\n");

_ = Task.Run(() => processInputs());
using var engine = TradingHostBuilder.BuildTradingServer();
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




