using System;
using Microsoft.Extensions.Logging;

namespace TradingServer.Logging 
{
    public record LogInformation(message_types type, string module, string msg, 
    DateTime now, int id, string name);
}

