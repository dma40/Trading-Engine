
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

namespace TradingServer.Logging {
    public class TextLogger: AbstractLogger, ITextLogger {

        private readonly LoggerConfiguration _logConfig;

        public TextLogger(IOptions<LoggerConfiguration> logConfig): base() {
            _logConfig = logConfig.Value ?? throw new ArgumentNullException();
            if (_logConfig.LoggerType != LoggerTypes.Text) {
                throw new InvalidOperationException("You can't initialize a TextLogger in this way. That is the wrong type");
            }
            
            var now = DateTime.Now;
            string logdir = Path.Combine(_logConfig.TextLoggerConfiguration.Directory, $"{now:yyyy-MM-dd}");
            string baseName = Path.Combine($"{_logConfig.TextLoggerConfiguration.Filename}-{now:HH-mm-ss}",
                                            _logConfig.TextLoggerConfiguration.FileExtension);
            string filepath = Path.Combine(logdir, baseName);

            Directory.CreateDirectory(logdir);

            _ = Task.Run(() => LogAsync(filepath, _logQueue, _ts.Token));
        }

        private static async Task LogAsync(string file_path, BufferBlock<LogInformation> logs, CancellationToken token) {
            using var filestream = new FileStream(file_path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            using var streamwriter = new StreamWriter(filestream) {AutoFlush = true};
            try {
                while (true) {
                    var logItem = await logs.ReceiveAsync(token).ConfigureAwait(false);
                    string formatted = FormatLogItem(logItem);
                    await streamwriter.WriteAsync(formatted).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) {
                Console.WriteLine("Something went wrong!");
            }
        }

        protected override void Log(message_types type, string module, string msg)
        {
            _logQueue.Post(new LogInformation(type, module, msg, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
        }

        private static string FormatLogItem(LogInformation log) {
            return $"[{log.now:HH-mm-ss.ffffff yyyy-MM-dd}]";
        }

        ~TextLogger() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean dispose) {
            if (_disposed) {
                return;
            }
            _disposed = true;

            if (dispose) {
                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private readonly BufferBlock<LogInformation> _logQueue = new BufferBlock<LogInformation>();
        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}