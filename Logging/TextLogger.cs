using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

namespace TradingServer.Logging 
{
    public class TextLogger: AbstractLogger, ITextLogger 
    {
        private readonly LoggerConfiguration _logConfig;

        public TextLogger(IOptions<LoggerConfiguration> logConfig): base() 
        {
            var log = new LoggerConfiguration 
            {
                LoggerType = LoggerType.Text,
                TextLoggerConfiguration = new TextLoggerConfiguration 
                {
                    Directory = "/Users/dylan.ma/Documents/Trading-Engine",
                    Filename = "TradingLogFile",
                    FileExtension = ".log"
                }
            };

            _logConfig = log ?? throw new ArgumentNullException();
            if (_logConfig.LoggerType != LoggerType.Text) 
            {
                throw new InvalidOperationException("You can't initialize a TextLogger in this way. That is the wrong type");
            }
            
            var now = DateTime.Now;
            string logdir = Path.Combine(_logConfig.TextLoggerConfiguration.Directory, $"{now:yyyy-MM-dd}");
            string filename = $"{_logConfig.TextLoggerConfiguration.Filename}-{now:HH-mm-ss}";
            string logbase = Path.ChangeExtension(filename, _logConfig.TextLoggerConfiguration.FileExtension);

            string filepath = Path.Combine(logdir, logbase);

            Directory.CreateDirectory(logdir);

            _ = Task.Run(() => LogAsync(filepath, _logQueue, _ts.Token));
        }

        private static async Task LogAsync(string filepath, BufferBlock<LogInformation> logs, CancellationToken token) 
        {
            using var filestream = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var streamwriter = new StreamWriter(filestream) {AutoFlush = true}; // maybe do FileMode.Append to allow new entries to be written in, instead of CreateNew
            // also maybe try making sure the task is running before appending new entries
            try 
            {
                while (!token.IsCancellationRequested) 
                {
                    var logItem = await logs.ReceiveAsync(token).ConfigureAwait(false);
                    string formatted = FormatLogItem(logItem);
                    await streamwriter.WriteAsync(formatted).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) 
            {
                Console.WriteLine("Something went wrong!");
            }
        }

        protected override void Log(message_types type, string module, string msg)
        {
            _logQueue.Post(new LogInformation(type, module, msg, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
        }

        private static string FormatLogItem(LogInformation log) 
        {
            return $"[{log.now:HH-mm-ss.ffffff yyyy-MM-dd} {log.type} - {log.module}: {log.msg}]\n";
        }

        ~TextLogger() 
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean dispose) 
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