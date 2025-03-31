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

        services.AddSingleton<TradingClient>(); // because TradingServer handles order processing
        // services.AddSingleton<TradingClient>();

        services.AddSingleton<ITextLogger, TextLogger>();
        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>(); // this is because TradingServer is a hosted service - there can only be one 

        // figure out how to register TradingClient as a service, this seems to be a error
        services.AddGrpc();
    })
    .Build();
}
