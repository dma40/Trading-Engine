namespace TradingServer.Orders
{
    public sealed class Trade
    {
        public Trade(OrderRecord _incoming, OrderRecord _resting)
        {
            incoming = _incoming;
            resting = _resting;
        }

        public OrderRecord incoming { get; private set; }
        public OrderRecord resting { get; private set; }
    }
}