
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using TradingServer.Core;
using Trading;

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
        Side = "Bid",
        Operation = "Add", // other options are Modify, Remove
        Username = "Dylan" // who placed this order
    };

    var orderRequest2 = new OrderRequest
    {
        Id = 400,
        Quantity = 200,
        Price = 100,
        Side = "Bid",
        Operation = "Add",
        Username = "Dylan"
    };

    /*
    var orderRequest3 = new OrderRequest
    {
        Id = 500,
    };
    */

    // this comprises the interactive client so far

    var response = await tradingService.ProcessOrderAsync(orderRequest);
    var response2 = await tradingService.ProcessOrderAsync(orderRequest2);

    await engine.RunAsync().ConfigureAwait(false);
}




