using System;

namespace TradingServer.Logging.LoggingConfiguration {
    public class LoggerConfiguration 
    {
        public LoggerType LoggerType { get; set; }
        public TextLoggerConfiguration TextLoggerConfiguration { get; set; }
    }

    public class TextLoggerConfiguration 
    {
        public string Directory { get; set; }

        public string Filename { get; set; }

        public string FileExtension { get; set; }
    }
}