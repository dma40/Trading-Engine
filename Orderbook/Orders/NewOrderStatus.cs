using System;

namespace TradingServer.Orders 
{
    public class NewOrderStatus 
    {
        public NewOrderStatus(bool isProcessed)
        {
            _processed = isProcessed;
        }
        private bool _processed;
    }
}