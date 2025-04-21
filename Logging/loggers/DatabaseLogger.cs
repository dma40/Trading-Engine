using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

using MySql.Data.MySqlClient;

namespace TradingServer.Logging
{
    public class DatabaseLogger: AbstractLogger, ITextLogger
    {
        private readonly LoggerConfiguration _logConfig;

        public DatabaseLogger(IOptions<LoggerConfiguration> logConfig)
        {
            _logConfig = logConfig.Value ?? throw new ArgumentNullException();
            
            if (_logConfig.LoggerType != LoggerType.Database) 
            {
                throw new InvalidOperationException("You can't initialize a DatabaseLogger in this way. That is the wrong type");
            }

            DateTime now = DateTime.UtcNow;

            string? user = Environment.GetEnvironmentVariable("MYSQL_USER");
            string? password = Environment.GetEnvironmentVariable("MYSQL_PASS");
            string db = $"CREATE DATABASE IF NOT EXISTS {_logConfig.TextLoggerConfiguration.Filename}-{now:yyyy-MM-dd}";
            string dbname = $"{_logConfig.TextLoggerConfiguration.Filename}-{now:yyyy-MM-dd}";
            string dblink = $"Server=host.docker.internal;Database={dbname};User={user};Password={password}";
            string link = $"Server=host.docker.internal;User ID={user};Password={password}";

            string createTableRequest = @"
            CREATE TABLE IF NOT EXISTS LogInformation (
                type INT NOT NULL,
                module VARCHAR(100) NOT NULL,
                message VARCHAR(200) NOT NULL,
                now TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                id INT NOT NULL,
                name VARCHAR(100) NOT NULL
            )";

            using (var conn = new MySqlConnection(link))
            {
                conn.Open();
                using (var command = new MySqlCommand(db, conn))
                {
                    command.ExecuteNonQueryAsync();

                    var connection = new MySqlConnection(dblink);
                    using (var request = new MySqlCommand(createTableRequest, connection))
                    {
                        request.ExecuteNonQueryAsync();
                    }
                }
                _ = Task.Run(() => LogAsync(dblink, _logQueue, _ts.Token));
            }
        }

        private static async Task LogAsync(string db, BufferBlock<LogInformation> logs, CancellationToken token)
        {
            MySqlConnection connection = new MySqlConnection(db);

            try 
            {
                while (!token.IsCancellationRequested)
                {
                    var item = await logs.ReceiveAsync(token).ConfigureAwait(false);
                    string request = FormatLogItem(item);
                    using (var command = new MySqlCommand(request, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            catch (OperationCanceledException)
            {
                Console.WriteLine("Something went wrong!");
            }
        }

        protected override void Log(message_types type, string module, string message)
        {
            _logQueue.Post(new LogInformation(type, module, message, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
        }

        private static string FormatLogItem(LogInformation log)
        {
            return "INSERT INTO LogInformation (type, module, message, now, id, name) " 
            + $"VALUES ({log.type}, {log.module}, {log.message}, {log.now}, {log.id}, {log.name})";
        }

        ~DatabaseLogger() 
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