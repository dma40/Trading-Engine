using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TradingServer.OrderbookCS;
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

        }
    }
}