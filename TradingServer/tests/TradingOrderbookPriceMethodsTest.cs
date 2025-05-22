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
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }

        [Test]
        /* Runs outside of market hours */
        public void skipsUpdateOutsideOfHours()
        {

        }
    }
}