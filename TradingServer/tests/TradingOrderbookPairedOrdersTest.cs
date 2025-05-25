using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TradingServer.OrderbookCS;
using TradingServer.Orders;
using TradingServer.Instrument;
using NUnit.Framework;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookPairedOrdersTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public async Task PairedExecutionOrderTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }
            
            // IOrderCore primaryCore = new OrderCore()
            // IOrderCore secondaryCore = new OrderCore()
            // Order primary =
            // Order secondary = 
            // PairedExecutionOrder peo = new PairedExecutionOrder(core, primary, secondary);

            // test various situations
        }

        [Test]
        public async Task PairedCancelOrderTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }

            // IOrderCore primaryCore = new OrderCore()
            // IOrderCOre secondaryCore = new OrderCore()
            // Order primary = 
            // Order secondary = 
            // PairedCancelOrder pco = new PairedCancelOrder(core, primary, secondary);

            // test each type of matching scenario
        }
    }
}