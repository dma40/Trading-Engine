using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using TradingServer.OrderbookCS;
using TradingServer.Orders;
using TradingServer.Instrument;
using NUnit.Framework;

namespace TradingServer.Tests
{
    public class TradingOrderbookPairedOrdersTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Fact]
        public void PairedExecutionOrderTest()
        {
            // Order primary =
            // Order secondary = 
            // PairedExecutionOrder peo = new PairedExecutionOrder(core, primary, secondary);

            // test various situations
        }

        [Fact]
        public void PairedCancelOrderTest()
        {
            // Order primary = 
            // Order secondary = 
            // PairedCancelOrder pco = new PairedCancelOrder(core, primary, secondary);

            // test each type of matching scenario
        }
    }
}