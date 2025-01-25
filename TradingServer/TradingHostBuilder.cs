using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingServer.Core.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

namespace TradingServer.Core;

public class TradingHostBuilder {
    public static IHost BuildTradingServer() => Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
    {
        services.AddOptions();
        services.Configure<TradingServerConfiguration>(context.Configuration.GetSection(nameof(TradingServerConfiguration)));

        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>();
    })
    .Build();
}
