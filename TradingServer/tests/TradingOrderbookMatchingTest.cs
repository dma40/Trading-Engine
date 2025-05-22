using System.Collections.Generic;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using Xunit;
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

        [Fact]
        public void FillAndKillTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Fact]
        public void ImmediateHandleTypeMatched()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Fact]
        public void PostOnlyMatch()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Fact]
        public void HiddenOrderAddedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Fact]
        public void VisibleOrderAddedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Fact]
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