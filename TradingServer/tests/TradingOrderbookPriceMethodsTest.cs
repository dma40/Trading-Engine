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
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public void TestPriceUpdatedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore buyCore = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                IOrderCore sellCore = new OrderCore(i + 20000, "Dylan", "TEST", OrderTypes.GoodTillCancel);

                _tradingEngine.addOrder(new Order(buyCore, i / 4, 1, false));
                _tradingEngine.addOrder(new Order(sellCore, i / 4, 1, true));

                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(16, 0, 0);

                TimeSpan now = DateTime.Now.TimeOfDay;

                if (now >= marketOpen && now <= marketEnd)
                {
                    Assert.That(_tradingEngine.lastTradedPrice == i / 4);
                }

                else
                {
                    Assert.That(_tradingEngine.lastTradedPrice == -1);
                }
            }
        }
    }
}