
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using TradingServer.Core;
using Trading;
using System.Diagnostics;

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
        Type = "GoodTillCancel",
        Side = "Bid",
        Operation = "Add", // other options are Modify, Remove
        Username = "Dylan" // who placed this order
    };

    var orderRequest2 = new OrderRequest
    {
        Id = 400,
        Quantity = 200,
        Price = 100,
        Type = "GoodTillCancel",
        Side = "Bid",
        Operation = "Add",
        Username = "Dylan"
    };

    var orderRequest3 = new OrderRequest
    {
        Id = 126,
        Quantity = 300,
        Price = 50,
        Type = "GoodTillCancel",
        Side = "Ask",
        Operation = "Add",
        Username = "Dylan"
    };

    await tradingService.ProcessOrderAsync(orderRequest);
    await tradingService.ProcessOrderAsync(orderRequest2);
    // await tradingService.ProcessOrderAsync(orderRequest3);

    // stub for the interactive client
    Console.WriteLine("Input something");
    string input = Console.ReadLine();

    // frame for the rest of the input that handles the input the user provided
    if (input == "Add")
    {
        var request = new OrderRequest
        {
            Id = 600,
            Operation = "Add",
            Quantity = 500,
            // Type = "GoodTillCancel",
            Side = "Ask",
            Price = 320,
            Username = "Dylan"
        };
    }

    else if (input == "Cancel")
    {
        var orderRequest4 = new OrderRequest
        {
            Id = 500,
            Operation = "Cancel",
            Username = "Dylan",
            Side = "Bid" // for this one don't take a input - instead try and find the order, and then determine its side.
        };
        // cancelling seems to work when all the orders are from one side, doesn't work when there's a match apparently
        await tradingService.ProcessOrderAsync(orderRequest4);
    }

    else if (input == "Modify")
    {
        var orderRequest5 = new OrderRequest
        {
            Id = 500,
            Operation = "Modify",
            Side = "Bid",
            Username = "Dylan",
            Price = 120
        };

        // the fact that there was no side, caused this to have a problem...

        await tradingService.ProcessOrderAsync(orderRequest5);
    }

    Console.WriteLine("You entered " + input);
    
    await engine.RunAsync().ConfigureAwait(false);
}




