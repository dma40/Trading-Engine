using GrpcServices.Services;

var server = new Server;
var channel = GrpcChannel.ForAddress("https://localhost:12000");

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;
{
    using var scope = TradingServerServiceProvider.serviceProvider.CreateScope();
    await engine.RunAsync().ConfigureAwait(false);
}
