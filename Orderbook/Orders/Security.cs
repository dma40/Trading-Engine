namespace TradingServer.Instrument
{
    public sealed class Security
    {
        public Security(string Name, string SecurityID)
        {
            name = Name;
            id = SecurityID;
        }

        public string name { get; private set; }
        public string id { get; private set; }
    }
}