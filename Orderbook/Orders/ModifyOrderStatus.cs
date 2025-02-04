using System;

namespace TradingServer.Orders 
{
    public class ModifyOrderStatus 
    {
        public ModifyOrderStatus(bool modified)
        {
            _processed = modified;
        }
        
        private bool _processed;
    }
}