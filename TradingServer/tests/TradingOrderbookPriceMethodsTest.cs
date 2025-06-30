using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using TradingServer.Instrument;
using NUnit.Framework.Constraints;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookPriceMethodsTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST", "TEST_ID"));
        }

        [Test]
        public void TestPriceUpdatedCorrectly()
        {
            const int count = 1000000;
            /*
            for (int i = 0; i < count; i++)
            {
                IOrderCore buyCore = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(buyCore, i / 4, 1000, false));
            }
            */

            /*
            Console.WriteLine($"Finished adding {count} orders. ");
            var usage = GC.GetTotalMemory(forceFullCollection: false);
            Console.WriteLine($"Total memory used: {usage}");
            */

            for (int i = 0; i < count; i++)
            {
                IOrderCore buyCore = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(buyCore, i / 4, 1000, false));
            }

            Console.WriteLine($"Finished adding {count} orders. ");
            var usage = GC.GetTotalMemory(forceFullCollection: false);
            Console.WriteLine($"Total memory used: {usage}");

            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                IOrderCore sellCore = new OrderCore(i + count, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(sellCore, i / 4, 1000, true));

                //IOrderCore buyCore = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
               // _tradingEngine.addOrder(new Order(buyCore, i / 4, 1000, false));

                //IOrderCore buyCore = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                //_tradingEngine.addOrder(new Order(buyCore, i / 4, 1000, false));
                // Console.WriteLine(_tradingEngine.lastTradedPrice);
                /*
                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(16, 0, 0);

                TimeSpan now = DateTime.Now.TimeOfDay;

                if (now >= marketOpen && now <= marketEnd)
                {
                    Console.WriteLine(_tradingEngine.lastTradedPrice);
                    Assert.That(_tradingEngine.lastTradedPrice == i / 4);
                }

                else
                {
                    Assert.That(_tradingEngine.lastTradedPrice == -1);
                }
                */
            }

            watch.Stop();
            var used = GC.GetTotalMemory(forceFullCollection: false);
            Console.WriteLine($"Total memory used: {used}");
            Console.WriteLine($"Elapsed time: " + watch.ElapsedMilliseconds);
        }
    }
}