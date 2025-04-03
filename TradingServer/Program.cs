
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using TradingServer.Core;
using Trading;

/*
using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;
{
    using var scope = TradingServerServiceProvider.serviceProvider.CreateScope();
    await engine.RunAsync().ConfigureAwait(false);
}
*/

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

using (var scope = TradingServerServiceProvider.serviceProvider.CreateScope())
{
    var tradingService = scope.ServiceProvider.GetRequiredService<TradingClient>();

    var orderRequest = new OrderRequest
    {
        Id = 500,
        Quantity = 300,
        Price = 100,
        Operation = "sell"
    };

    var response = await tradingService.ProcessOrderAsync(orderRequest);
    await engine.RunAsync().ConfigureAwait(false);
}


// Only thing left: create a interactive client that allows the user to add, edit, remove orders.



