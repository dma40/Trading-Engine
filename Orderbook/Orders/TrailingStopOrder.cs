namespace TradingServer.Orders
{
    public class TrailingStopOrder: Order
    {
        public TrailingStopOrder(IOrderCore _orderCore, long _price, uint _quantity, bool _isBuy, OrderTypes _orderType): 
        base(_orderCore, _price, _quantity, _isBuy, _orderType)
        {

        }

        public long trail { get; private set; }
    }
}