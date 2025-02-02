
using System;

namespace TradingServer.Orderbook
{

    public interface IReadOnlyOrderbook 
    {
        bool containsOrder(long orderID);
        OrderbookSpread spread();
        int count { get; }
    }
}
