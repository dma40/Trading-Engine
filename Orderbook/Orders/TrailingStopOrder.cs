namespace TradingServer.Orders
{
    public class TrailingStopOrder: Order
    {
        static int ask = int.MinValue;
        static int bid = int.MaxValue;

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, uint _quantity, bool _isBuy, OrderTypes _orderType): 
        base(_orderCore, _isBuy ? bid : ask, _quantity, _isBuy, _orderType)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopMarket)
                throw new InvalidDataException();
            

            trail = _trail;
        }

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, long _price, uint _quantity, bool _isBuy, OrderTypes _orderType):
        base(_orderCore, _price, _quantity, _isBuy, _orderType)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopLimit)
                throw new InvalidDataException();
            

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