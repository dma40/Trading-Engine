using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using NUnit.Framework;
using TradingServer.OrderbookCS;
using TradingServer.Instrument;

namespace TradingServer.Tests
{
    public class TradingOrderbookPriceMethodsTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }


        [Fact]
        public void TestPriceUpdatedCorrectly()
        {

        }

        [Fact]
        /* Runs outside of market hours */
        public void skipsUpdateOutsideOfHours()
        {

        }
    }
}