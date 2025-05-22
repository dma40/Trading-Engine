using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using NUnit.Framework;
using Org.BouncyCastle.Security;
using TradingServer.Instrument;
using NUnit.Framework.Constraints;

namespace TradingServer.Tests
{
    public class TradingOrderbookStopOrderTest()
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        public void StopOrderAddedWhenGreatestPriceChanges()
        {

        }

        public void StopOrderTestChangedCorrectly()
        {

        }

        public void TrailingStopOrderMatchedWhenPriceChanges()
        {

        }
    }
}