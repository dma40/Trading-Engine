using System;

namespace TradingServer.OrderbookCS
{
    public interface IReadOnlyOrderbook 
    {
        bool containsOrder(long orderID);
        OrderbookSpread spread();
        int count { get; }
    }
}
