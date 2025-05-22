using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using NUnit.Framework;
using Xunit;
using TradingServer.Instrument;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookIcebergOrdersTest
    {
        private TradingEngine _tradingEngine;

        [Fact]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Fact]
        public void IcebergTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
        }
    }
}