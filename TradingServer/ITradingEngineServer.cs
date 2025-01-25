namespace TradingServer.Core {
    interface ITradingServer {
        Task Run(CancellationToken token);
    }
}