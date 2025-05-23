using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using TradingServer.Instrument;

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
                IOrderCore sellCore = new OrderCore(i + 200001, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(buyCore, i / 4, 1, false));
                _tradingEngine.addOrder(new Order(sellCore, i / 4, 1, true));

                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(16, 0, 0);

                TimeSpan now = DateTime.Now.TimeOfDay;

                if (now >= marketOpen && now <= marketEnd)
                {
                    Assert.That(_tradingEngine.lastTradedPrice == i / 4);
                }
            }

            /*
            IOrderCore currentQuantityCanBeFullyFilled = new OrderCore(20004, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order currentQuantityCanBeFullyFilledTest = new Order(currentQuantityCanBeFullyFilled, 10000, 19999, true);

            _tradingEngine.addOrder(currentQuantityCanBeFullyFilledTest);
            Assert.That(currentQuantityCanBeFullyFilledTest.CurrentQuantity == 0);

            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now <= new TimeSpan(16, 0, 0) && now >= new TimeSpan(9, 30, 0))
            {
                Assert.That(_tradingEngine.lastTradedPrice == 0);
            }
            */
        }

        [Test]
        /* Runs outside of market hours */
        public void skipsUpdateOutsideOfHours()
        {

        }
    }
}