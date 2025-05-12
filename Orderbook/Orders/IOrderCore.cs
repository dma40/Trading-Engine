namespace TradingServer.Orders 
{
    public interface IOrderCore
    {
        public long OrderID { get; }
        public string Username { get; }
        public string SecurityID { get; }
        public bool isHidden { get; }
        public OrderTypes OrderType { get; }
    }
}
