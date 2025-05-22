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
    /*
    public class TradingOrderbookIcebergOrdersTest
    {
        private readonly Mock<IMatchingEngine> _mockMatchingEngine;
        private readonly Mock<IDisposable> _mockDisposable;
        private readonly Mock<IOrder> _mockOrder;
        private readonly Mock<IOrderbook> _mockOrderbook;

        public TradingOrderbookIcebergOrdersTest()
        {
            _mockMatchingEngine = new Mock<IMatchingEngine>();
            _mockDisposable = new Mock<IDisposable>();
            _mockOrder = new Mock<IOrder>();
            _mockOrderbook = new Mock<IOrderbook>();
        }

        [Fact]
        public async Task TestProcessIcebergOrders()
        {
            // Arrange
            var mockOrder = new Mock<IOrder>();
            var mockOrderbook = new Mock<IOrderbook>();
            var mockMatchingEngine = new Mock<IMatchingEngine>();
            var mockDisposable = new Mock<IDisposable>();
            var mockOrderbook = new Mock<IOrderbook>();
        }
    }
    */
}