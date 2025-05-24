using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using NUnit.Framework;
using TradingServer.Instrument;
using NUnit.Framework.Constraints;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookStopOrderTest()
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public void StopOrderAddedWhenLatestPriceChanges()
        {
            IOrderCore stopCore = new OrderCore(40000, "Dylan", "TEST", OrderTypes.StopLimit);
            StopOrder stop = new StopOrder(stopCore, 98, 100, 100, false);

            _tradingEngine.addOrder(stop);

            Assert.That(!_tradingEngine.orderbook.containsOrder(stop.OrderID));

            for (int i = 0; i < 404; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                IOrderCore sellCore = new OrderCore(i + 404, "Dylan", "TEST", OrderTypes.GoodTillCancel);

                _tradingEngine.addOrder(new Order(core, i / 4, 1, true));
                _tradingEngine.addOrder(new Order(sellCore, i / 4, 1, false));
                Assert.That(_tradingEngine.lastTradedPrice == i / 4);
            }

            Assert.That(_tradingEngine.orderbook.containsOrder(stop.OrderID));
        }

        [Test]
        public void StopOrderTestChangedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        public void TrailingStopOrderMatchedWhenPriceChanges()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }
    }
}