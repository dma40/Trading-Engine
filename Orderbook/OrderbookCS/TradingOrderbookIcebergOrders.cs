using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Dictionary<long, IcebergOrder> _iceberg = new Dictionary<long, IcebergOrder>();

        protected void ProcessIcebergOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                //bool acquired = _semaphore.Wait(TimeSpan.FromMilliseconds(100), token);

                //if (acquired)
                lock (_ordersLock)
                {
                    if (currentTime >= marketOpen && currentTime <= marketEnd)
                    {
                        foreach (var order in _iceberg)
                        {
                            var iceberg = order.Value;

                            if (iceberg.CurrentQuantity == 0 && !iceberg.isEmpty)
                            {
                                iceberg.replenish();
                                addOrder(iceberg);
                            }

                            else if (iceberg.isEmpty)
                            {
                                _iceberg.Remove(iceberg.OrderID);
                            }
                        }
                    }

                    else
                    {
                        foreach (var order in _iceberg)
                        {
                            IcebergOrder iceberg = order.Value;

                            if (iceberg.isEmpty)
                            {
                                // Console.WriteLine("Iceberg is currently empty");
                                 _iceberg.Remove(iceberg.OrderID);
                            }

                                /*
                                else if (iceberg.CurrentQuantity == 0)
                                {
                                    Console.WriteLine("There is a empty iceberg order, but there is invisible quantity remaining.");
                                    iceberg.replenish();
                                    addOrder(iceberg);
                                    Console.WriteLine(iceberg.CurrentQuantity);
                                    Console.WriteLine(containsOrder(iceberg.OrderID));
                                }
                                */

                                /*
                                else
                                {
                                    Console.WriteLine("A non-empty iceberg order is in a internal queue.");
                                    iceberg.replenish();
                                    addOrder(iceberg);
                                }
                                */
                        }
                    }
                }

                //_semaphore.Release();

                // await Task.Delay(200, token);
            }
        }
    }
}