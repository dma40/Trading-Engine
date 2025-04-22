using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

namespace TradingServer.Logging
{
    public class ConsoleLogger: AbstractLogger, ITextLogger, IDisposable
    {
        private readonly LoggerConfiguration _logConfig;

        public ConsoleLogger(IOptions<LoggerConfiguration> logConfig): base()
        {
            _logConfig = logConfig.Value ?? throw new ArgumentNullException("This cannot be null");

            if (_logConfig.LoggerType != LoggerType.Console)
            {
                throw new InvalidDataException();
            }

            _ = Task.Run(() => LogAsync(_logQueue, _ts.Token));
        }

        private async Task LogAsync(BufferBlock<LogInformation> logs, CancellationToken token)
        {
            try 
            {
                while (!token.IsCancellationRequested)
                {
                    var log = await logs.ReceiveAsync(token).ConfigureAwait(false);
                    var formatted = FormatLogItem(log);
                    Console.WriteLine(formatted);
                }
            }

            catch (OperationCanceledException)
            {
                Console.WriteLine("Something went wrong!");
            }
        }

        private static string FormatLogItem(LogInformation log) 
        {
            return $"[{log.now:HH-mm-ss.ffffff yyyy-MM-dd} {log.type} - {log.module}: {log.message}]\n";
        }

        protected override void Log(message_types type, string module, string message)
        {
            _logQueue.Post(new LogInformation(type, module, message, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name ?? "UnknownThread"));
        }

        ~ConsoleLogger() 
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose) 
        {
            if (_disposed) 
            {
                return;
            }

            _disposed = true;

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private readonly BufferBlock<LogInformation> _logQueue = new BufferBlock<LogInformation>();
        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}