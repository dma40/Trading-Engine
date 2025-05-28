namespace TradingServer.Orders
{
    public class TrailingStopOrder: Order, IOrderCore
    {
        static int ask = -1;
        static int bid = int.MaxValue;

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, uint _quantity, bool _isBuy): 
        base(_orderCore, _isBuy ? bid : ask, _quantity, _isBuy)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopMarket)
            {
                throw new InvalidDataException();
            }

            if (_orderCore.isHidden)
            {
                throw new InvalidDataException();
            }
            
            trail = _trail;
        }

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, long _price, uint _quantity, bool _isBuy):
        base(_orderCore, _price, _quantity, _isBuy)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopLimit)
            {
                throw new InvalidDataException();
            }
            
            trail = _trail;
        }

        public long trail { get; private set; }
        
        public new long StopPrice 
        {  
            get
            {
                return StopPrice;
            }

            set
            {
                if (isBuySide)
                {
                    StopPrice = currentMaxPrice - trail;
                }

                else
                {
                    StopPrice = currentMaxPrice + trail;
                }
            } 
        }

        public new long currentMaxPrice { get; set; }

        public sealed override Order activate()
        {
            return new Order(this);
        }

        ~TrailingStopOrder()
        {
            
        }
    }
}