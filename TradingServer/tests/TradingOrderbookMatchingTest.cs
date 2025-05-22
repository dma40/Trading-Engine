using System.Collections.Generic;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using Xunit;
using TradingServer.Instrument;
using NUnit.Framework;

namespace TradingServer.Tests
{
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

        }

        [Fact]
        public void ImmediateHandleTypeMatched()
        {

        }

        [Fact]
        public void PostOnlyMatch()
        {

        }

        [Fact]
        public void HiddenOrderAddedCorrectly()
        {

        }

        [Fact]
        public void VisibleOrderAddedCorrectly()
        {

        }

        [Fact]
        public void HasEligibleOrderTest()
        {

        }
    }
}