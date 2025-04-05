using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingServer.Core.Configuration;
using TradingServer.Logging;
using TradingServer.Handlers;

using Trading;

namespace TradingServer.Core;

public class TradingHostBuilder 
{
    public static IHost BuildTradingServer() => Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
    {
        services.AddOptions();
        services.Configure<TradingServerConfiguration>(context.Configuration.GetSection(nameof(TradingServerConfiguration)));

        services.AddSingleton<TradingClient>(); 
        // This works because of how ITradingServer is resolved at runtime

        services.AddSingleton<ITextLogger, TextLogger>();
        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>(); // this is because TradingServer is a hosted service

        services.AddGrpc();
    })
    .Build();
}

// Now that it appears that messages can be sent properly, it is time to start doing some of the 
// logic with respect to adding/removing/modifying orders from the buy/sell sides
