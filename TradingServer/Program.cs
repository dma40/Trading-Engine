using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using TradingServer.Core;

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

using (var scope = TradingServerServiceProvider.serviceProvider.CreateScope())
{
    await engine.RunAsync().ConfigureAwait(false);
}




