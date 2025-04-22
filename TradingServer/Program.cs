using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TradingServer.Tests;

using TradingServer.Core;

OrderbookUnitTests.runTests();

Console.WriteLine("Starting trading server...\n");

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

using (var scope = TradingServerServiceProvider.serviceProvider.CreateScope())
{
    await engine.RunAsync().ConfigureAwait(false);
}




