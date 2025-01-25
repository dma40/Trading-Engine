using System;
using System.Text;
using System.Collections.Generic;

namespace TradingServer.Core.Configuration {
    class TradingServerConfiguration {
        public TradingServerSettings TradingServerSettings { get; set; }
    }

    class TradingServerSettings {
        public int Port { get; set; }
    }
}