﻿using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IReadOnlyOrderbook 
    {
        bool containsOrder(long orderID);
        int count { get; }
        OrderbookSpread spread();

        public List<OrderbookEntry> getBidOrders()
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        public List<OrderbookEntry> getAskOrders()
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        public void addOrder(Order order)
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        public void modifyOrder(ModifyOrder modify)
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        public void removeOrder(CancelOrder cancel)
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }  

        public Trades match(Order order)
        {
            throw new UnauthorizedAccessException("401 UnauthorizedAccess");
        }
    }
}
