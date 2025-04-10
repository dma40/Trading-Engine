
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using TradingServer.Core;
using Trading;

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;

using (var scope = TradingServerServiceProvider.serviceProvider.CreateScope())
{
    var tradingService = scope.ServiceProvider.GetRequiredService<TradingClient>();

    // Add a few simple tests

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

    var orderRequest3 = new OrderRequest
    {
        Id = 126,
        Quantity = 300,
        Price = 50,
        Side = "Ask",
        Operation = "Add",
        Username = "Dylan"
    };

    await tradingService.ProcessOrderAsync(orderRequest);
    await tradingService.ProcessOrderAsync(orderRequest2);
    //await tradingService.ProcessOrderAsync(orderRequest3);

    // stub for the interactive client
    Console.WriteLine("Input something");
    string input = Console.ReadLine();

    // frame for the rest of the input that handles the input the user provided
    if (input == "Add")
    {
        
    }

    else if (input == "Cancel")
    {
        var orderRequest4 = new OrderRequest
        {
            Id = 500,
            Operation = "Cancel",
            Username = "Dylan"
        };
        // cancelling seems to work when all the orders are from one side, doesn't work when there's a match apparently
        await tradingService.ProcessOrderAsync(orderRequest4);
    }

    else if (input == "Modify")
    {

    }

    Console.WriteLine("You entered " + input);
    
    await engine.RunAsync().ConfigureAwait(false);
}




