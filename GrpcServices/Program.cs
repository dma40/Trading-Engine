using GrpcServices.Services;

var server = new Server;
var channel = GrpcChannel.ForAddress("https://localhost:14000");
//var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddGrpc();

//var app = builder.Build();

// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

//app.Run();

// Start up the server, add a gRPC communicator to that 

using var engine = TradingHostBuilder.BuildTradingServer();
TradingServerServiceProvider.serviceProvider = engine.Services;
{
    using var scope = TradingServerServiceProvider.serviceProvider.CreateScope();
    await engine.RunAsync().ConfigureAwait(false);
}
