using System;

// Extra: DBLogger, ConsoleLogger

namespace TradingServer.Logging {

    public enum message_types
    {
        Debug,
        LogInformation,
        Warning,
        Error
    }

    public enum LoggerType {
        Text,
        Database
    }
}
