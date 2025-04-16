
namespace TradingServer.Orders 
{
    public class CancelOrderStatus 
    {
        public CancelOrderStatus(bool cancel)
        {   
            _processed = cancel;
        }

        private bool _processed;
    }
}