using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

using MySql.Data.MySqlClient;

namespace TradingServer.Logging
{
    public class DatabaseLogger: AbstractLogger, ITextLogger, IDisposable
    {
        private readonly LoggerConfiguration _logConfig;

        public DatabaseLogger(IOptions<LoggerConfiguration> logConfig): base()
        {
            _logConfig = logConfig.Value ?? throw new ArgumentNullException();

            if (_logConfig.LoggerType != LoggerType.Database)
            {
                throw new InvalidOperationException("You can't initialize a DatabaseLogger in this way");
            }

            DateTime now = DateTime.Now;

            string? user = Environment.GetEnvironmentVariable("MYSQL_USER");
            string? password = Environment.GetEnvironmentVariable("MYSQL_PASS");

            string? filename = _logConfig?.TextLoggerConfiguration?.Filename ?? throw new ArgumentException("Filename cannot be null");

            string dbname = $"{filename}_{now.Year}_{now.Month}_{now.Day}"; // maybe include the time too
            string dbquery = $"CREATE DATABASE IF NOT EXISTS {dbname};";
            string link = $"Server=localhost;Port=3306;Uid={user};Pwd={password}";
            string dblink = $"Server=localhost;Port=3306;Database={dbname};Uid={user};Pwd={password}";
            
            string createTableRequest = @"
            CREATE TABLE IF NOT EXISTS LogInformation (
                type INT NOT NULL PRIMARY KEY,
                module VARCHAR(100) NOT NULL,
                message VARCHAR(200) NOT NULL,
                now TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                id INT NOT NULL,
                name VARCHAR(100) NOT NULL
            );";


            using (var conn = new MySqlConnection(link))
            {
                conn.Open();

                using (var command = new MySqlCommand(dbquery, conn))
                {
                    command.ExecuteNonQuery();

                    var connection = new MySqlConnection(dblink);
                    connection.Open();

                    using (var request = new MySqlCommand(createTableRequest, connection))
                    {
                        request.ExecuteNonQuery();
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
                Console.WriteLine("Logger has shut down.");
            }
        }

        protected override void Log(message_types type, string module, string message)
        {
            _logQueue.Post(new LogInformation(type, module, message, DateTime.Now, 
            Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name ?? "UnknownThread"));
        }

        private static string FormatLogItem(LogInformation log)
        {
            return "INSERT INTO LogInformation (type, module, message, now, id, name) " 
            + $"VALUES ({log.type}, {log.module}, {log.message}, {log.now}, {log.id}, {log.name});";
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