using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook
    {
        private readonly Security _security;

        public Orderbook(Security instrument)
        {
            _security = instrument;

            _router = new RestingRouter();
        }

        ~Orderbook()
        {
            
        } 
    }
}