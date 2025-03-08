using GrpcServices.Services;

// redo things

var server = new Server;
var channel = GrpcChannel.ForAddress("https://localhost:12000");

// maybe no need to manually reinject this process?

//using var engine = TradingHostBuilder.BuildTradingServer();
//TradingServerServiceProvider.serviceProvider = engine.Services;
//{
//   using var scope = TradingServerServiceProvider.serviceProvider.CreateScope();
//    await engine.RunAsync().ConfigureAwait(false);
//}

public class Program 
{
    public static void Program(string[] args)
    {

    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.addOptions();

                services.AddSingleton<ITextLogger, TextLogger>();
                services.AddSingleton<IMatchingOrderbook, FIFOrderbook>();

                services.Configure<TradingServerConfiguration>(context.Configuration.GetSection("TradingServerConfiguration"));

                services.AddGrpc();

                services.AddHostedService<TradingServer>();
            }).Build();
}
