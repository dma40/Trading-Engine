using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using TradingServer.Logging.LoggingConfiguration;

using Npgsql;

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

            string? user = Environment.GetEnvironmentVariable("SQL_USER");
            string? password = Environment.GetEnvironmentVariable("SQL_PASS");

            string? filename = _logConfig?.TextLoggerConfiguration?.Filename ?? throw new ArgumentException("Filename cannot be null");

            string dbname = $"{filename}_{now:yyyy-MM-dd}";
            string dbquery = @$"DO $$  
                                BEGIN

                                    IF NOT EXISTS (
                                        SELECT FROM pg_database WHERE name = '{dbname}'
                                    ) 

                                    THEN
                                        PERFORM dblink_exec('dbname={dbname}', 'CREATE DATABASE {dbname}')
                                    
                                    END IF;

                                END
                                $$";

            string link = $"Server=localhost;Port=5432;Uid={user};Pwd={password}";
            string dblink = $"Server=localhost;Port=5432;Database={dbname};Uid={user};Pwd={password}";
            
            string createTableRequest = @"
            CREATE TABLE IF NOT EXISTS LogInformation (
                type INT NOT NULL PRIMARY KEY,
                module VARCHAR(100) NOT NULL,
                message VARCHAR(200) NOT NULL,
                now TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                id INT NOT NULL,
                name VARCHAR(100) NOT NULL
            );";

            using (var conn = new NpgsqlConnection(link))
            {
                conn.Open();
                using (var command = new NpgsqlCommand(dbquery, conn))
                {
                    command.ExecuteNonQuery();

                    var connection = new NpgsqlConnection(dblink);
                    using (var request = new NpgsqlCommand(createTableRequest, connection))
                    {
                        request.ExecuteNonQuery();
                    }
                }

                _ = Task.Run(() => LogAsync(dblink, _logQueue, _ts.Token));
            }
        }

        private static async Task LogAsync(string db, BufferBlock<LogInformation> logs, CancellationToken token)
        {
            NpgsqlConnection connection = new NpgsqlConnection(db);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var item = await logs.ReceiveAsync(token).ConfigureAwait(false);
                    string request = FormatLogItem(item);
                    using (var command = new NpgsqlCommand(request, connection))
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