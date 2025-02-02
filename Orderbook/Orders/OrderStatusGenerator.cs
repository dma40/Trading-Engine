using System;

namespace TradingServer.Orders 
{
    public sealed class OrderStatusGenerator 
    {
        public static CancelOrderStatus GenerateCancel(CancelOrder cancel) 
        {
            return new CancelOrderStatus();
        }

        public static ModifyOrderStatus GenerateModify(ModifyOrder modify) 
        {
            return new ModifyOrderStatus();
        }

        public static NewOrderStatus GenerateNew(Order order) 
        {
            return new NewOrderStatus();
        }
    }
}