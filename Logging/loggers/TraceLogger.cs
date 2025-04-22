using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

namespace TradingServer.Logging
{
    public class TraceLogger: AbstractLogger, ITextLogger, IDisposable
    {
        private readonly LoggerConfiguration _logConfig;

        public TraceLogger(IOptions<LoggerConfiguration> logConfig): base()
        {
            _logConfig = logConfig.Value ?? throw new ArgumentNullException();

            if (_logConfig.LoggerType != LoggerType.Trace)
            {
                throw new InvalidDataException();
            }

            if (_logConfig.TextLoggerConfiguration == null)
            {
                throw new NullReferenceException(nameof(_logConfig.TextLoggerConfiguration));
            }

            DateTime now = DateTime.UtcNow;

            string logdir = Path.Combine(_logConfig.TextLoggerConfiguration.Directory 
                            ?? throw new NullReferenceException(), $"{now:yyyy-MM-dd}");
            string filename = $"{_logConfig.TextLoggerConfiguration.Filename}-{now:HH-mm-ss}";
            string logbase = Path.ChangeExtension(filename, _logConfig.TextLoggerConfiguration.FileExtension);

            string filepath = Path.Combine(logdir, logbase);

            Directory.CreateDirectory(logdir);

            _ = Task.Run(() => monitorThreadChanges());
            _ = Task.Run(() => LogAsync(filepath, _logQueue, _ts.Token));
        }

        private async Task LogAsync(string filepath, BufferBlock<LogInformation> logs, CancellationToken token)
        {
            using var filestream = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var streamwriter = new StreamWriter(filestream) {AutoFlush = true}; 

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

        private async Task monitorThreadChanges()
        {
            while (true)
            {
                while (!_ts.IsCancellationRequested)
                {
                    Process currentProcess = Process.GetCurrentProcess();

                    foreach (ProcessThread thread in currentProcess.Threads)
                    {
                        try 
                        {
                            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
                            {
                                Log(message_types.LogInformation, $"{thread.PriorityLevel}", $"{thread.ThreadState}");
                            }
                            else
                            {
                                Log(message_types.LogInformation, "UnsupportedPlatform", thread.ThreadState.ToString());
                            }
                        }

                        catch (Exception exception)
                        {
                            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
                            {
                                Log(message_types.Error, $"{thread.PriorityLevel}", $"{thread.ThreadState} {exception.Message}");
                            }
                            else
                            {
                                Log(message_types.Error, "UnsupportedPlatform", $"{thread.ThreadState} {exception.Message}");
                            }
                        }
                    }

                    await Task.Delay(200, _ts.Token);
                }
            }
        }

        protected override void Log(message_types type, string module, string message)
        {
            _logQueue.Post(new LogInformation(type, module, message, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name ?? "UnknownThread"));
        }

        private static string FormatLogItem(LogInformation log) 
        {
            return $"[{log.now:HH-mm-ss.ffffff yyyy-MM-dd} {log.type} - {log.module}: {log.message}]\n";
        }

        ~TraceLogger() 
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