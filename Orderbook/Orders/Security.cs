namespace TradingServer.Instrument
{
    public sealed class Security 
    {
        public Security(string Name)
        {
            name = Name;
        }

        public string name { get; private set; }
    }
}