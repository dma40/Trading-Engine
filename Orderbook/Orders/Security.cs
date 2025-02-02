using System.Dynamic;
using System;

namespace TradingServer.Instrument
{
    public class Security 
    {
        public Security(string Name)
        {
            name = Name;
        }

        public string name { get; private set; } // the name of the security, e.g. "Apple Inc."
    }
}