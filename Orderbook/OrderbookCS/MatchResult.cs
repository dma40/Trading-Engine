
using System;
using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public class MatchResult
    {
        public MatchResult() 
        {
            
        }

        public List<OrderRecord> result;
        // should be a list of order records
    }
}