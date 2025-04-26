using System.IO.Pipelines;

namespace TradingServer.OrderbookCS
{
    public enum PermissionLevel
    {
        ReadOnly,
        Retrieval,
        OrderEntry,
        Trading
    }

    public static class OrderbookPermissions
    {
        public static IReadOnlyOrderbook createOrderbookFromConfig(string security, PermissionLevel permission)
        {

            if (permission == PermissionLevel.ReadOnly)
            {
                IReadOnlyOrderbook result = new ReadOnlyOrderbook(new Instrument.Security(security));
                return result;
            }

            else if (permission == PermissionLevel.Retrieval)
            {          
                IRetrievalOrderbook result = new RetrievalOrderbook(new Instrument.Security(security));
                return result;
            }

            else if (permission == PermissionLevel.OrderEntry)
            {
                IOrderEntryOrderbook result = new OrderEntryOrderbook(new Instrument.Security(security));
                return result;
            }

            else 
            {
                ITradingOrderbook result = new TradingOrderbook(new Instrument.Security(security));
                return result;
            }
        }
    }
}