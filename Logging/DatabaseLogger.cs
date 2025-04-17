using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

using MySql.Data.MySqlClient;

namespace TradingServer.Logging
{
    public class DatabaseLogger//: AbstractLogger, ITextLogger
    {
        private readonly LoggerConfiguration _logConfig;

        public DatabaseLogger(IOptions<LoggerConfiguration> logConfig)
        {
            var log = new LoggerConfiguration 
            {
                LoggerType = LoggerType.Database,
                TextLoggerConfiguration = new TextLoggerConfiguration 
                {
                    Directory = "/Users/dylan.ma/Documents/Trading-Engine",
                    Filename = "TradingLogFile",
                    FileExtension = ".log"
                }
            };

            _logConfig = log ?? throw new ArgumentNullException();
            
            if (_logConfig.LoggerType != LoggerType.Database) 
            {
                throw new InvalidOperationException("You can't initialize a DatabaseLogger in this way. That is the wrong type");
            }

            string user = Environment.GetEnvironmentVariable("MYSQL_USER");
            string password = Environment.GetEnvironmentVariable("MYSQL_PASS");
            string connection = $"Server=localhost;User Id={user};Password={password}";

            string db = $"CREATE DATABASE IF NOT EXISTS {DateTime.Now:yyyy-MM-dd}";

            using (var conn = new MySqlConnection(connection))
            {
                conn.Open();
                _ = Task.Run(() => LogAsync(connection, _logQueue, _ts.Token));
            }
        }

        private static async Task LogAsync(string connection, BufferBlock<LogInformation> logs, CancellationToken token)
        {

        }

        private readonly BufferBlock<LogInformation> _logQueue = new BufferBlock<LogInformation>();
        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}