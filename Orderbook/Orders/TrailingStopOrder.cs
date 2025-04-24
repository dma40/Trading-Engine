namespace TradingServer.Orders
{
    public class TrailingStopOrder: Order
    {
        static int val = Int32.MinValue;

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, uint _quantity, bool _isBuy, OrderTypes _orderType): 
        base(_orderCore, val, _quantity, _isBuy, _orderType)
        {
            trail = _trail;
        }

        public long trail { get; private set; }
        public long StopPrice 
        {  
            get
            {
                return StopPrice;
            }

            set
            {
                if (isBuySide)
                    StopPrice = currentMaxPrice - trail;

                else
                    StopPrice = currentMaxPrice + trail;
            } 
        }

        public long currentMaxPrice { get; set; }

        public Order activate()
        {
            return new Order(this);
        }
    }
}