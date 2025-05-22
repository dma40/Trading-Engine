using System.Collections.Generic;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using TradingServer.Instrument;
using NUnit.Framework;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookMatchingTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public void FillAndKillTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        public void ImmediateHandleTypeMatched()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        public void PostOnlyMatch()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, true));
            }

            IOrderCore unmatchableCore = new OrderCore(20000, "Dylan", "TEST", OrderTypes.PostOnly);
            Order unmatchableOrder = new Order(unmatchableCore, 5001, 5001, false);

            _tradingEngine.match(unmatchableOrder);

            Assert.That(!_tradingEngine.containsOrder(unmatchableOrder.OrderID));
        }

        [Test]
        public void HiddenOrderAddedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        public void VisibleOrderAddedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        public void HasEligibleOrderTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }
    }
}