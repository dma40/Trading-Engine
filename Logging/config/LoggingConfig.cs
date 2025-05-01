
namespace TradingServer.Logging.LoggingConfiguration 
{
    public sealed class LoggerConfiguration 
    {
        public LoggerType LoggerType { get; set; }
        public TextLoggerConfiguration TextLoggerConfiguration { get; set; }
    }

    public sealed class TextLoggerConfiguration 
    {
        public string Directory { get; set; }
        public string Filename { get; set; }
        public string FileExtension { get; set; }
    }
}