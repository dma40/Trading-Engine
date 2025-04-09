using System;
using System.Text;
using System.Collections.Generic;

namespace TradingServer.Core.Configuration {
    class TradingServerConfiguration {
        public TradingServerSettings TradingServerSettings { get; set; }
    }

    class TradingServerSettings {
        public int Port { get; set; }
        public string SecurityName { get; set; } 
        public int SecurityID { get; set; }
        // a new instance attribute that stores the name of 
        // the security we have in our orderbook. Figure this out later
    }
}