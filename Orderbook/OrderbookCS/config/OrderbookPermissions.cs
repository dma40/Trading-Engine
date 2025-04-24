using System.Runtime.InteropServices;

namespace TradingServer.OrderbookCS
{
    public enum PermissionLevel
    {
        ReadOnly,
        OrderEntry,
        Retrieval,
        Matching
    }

    public static class OrderbookPermissions
    {
        public static IReadOnlyOrderbook createOrderbookFromConfig(string security, PermissionLevel permission)
        {
            IReadOnlyOrderbook result = new Orderbook(new Instrument.Security(security));

            if (permission == PermissionLevel.ReadOnly)
            {
                return result;
            }

            else if (permission == PermissionLevel.OrderEntry)
            {
                return (IOrderEntryOrderbook) result;
            }

            else if (permission == PermissionLevel.Retrieval)
            {          
                return (IRetrievalOrderbook) result;
            }

            else 
            {
                return (IMatchingOrderbook) result;
            }
        }
    }
}