using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TradingServer.OrderbookCS;
using TradingServer.Orders;

namespace TradingServer.Tests
{
    public class TimedOrderTests
    {

    }
    /*
    [TestFixture]
    public class TradingOrderbookTimedOrdersTest
    {
        private Mock<IMatchingEngine> _mockMatchingEngine;
        private Mock<IDisposable> _mockDisposable;
        private Mock<Orderbook> _mockOrderbook;
        private Mock<IHiddenOrderbook> _mockHiddenOrderbook;
        private TradingEngine _tradingEngine;
        private CancellationTokenSource _cts;

        [SetUp]
        public void Setup()
        {
            _mockMatchingEngine = new Mock<IMatchingEngine>();
            _mockDisposable = new Mock<IDisposable>();
            _mockOrderbook = new Mock<IOrderbook>();
            _mockHiddenOrderbook = new Mock<IHiddenOrderbook>();

            _cts = new CancellationTokenSource();

            _tradingEngine = new TradingEngine
            {
                _ts = _cts.Token,
                orderbook = _mockOrderbook.Object,
                _hidden = _mockHiddenOrderbook.Object,
                _stopLock = new object(),
                _onMarketOpen = new Dictionary<long, Order>(),
                _onMarketClose = new Dictionary<long, Order>()
            };
        }

        [Test]
        public async Task ProcessAtMarketEnd_ShouldDeleteOrdersAndProcessOnMarketEndOrders()
        {
            // Arrange
            var mockOrder = new Mock<Order>();
            _tradingEngine._onMarketClose.Add(1, mockOrder.Object);

            DateTime marketEndTime = DateTime.Today.Add(new TimeSpan(16, 0, 0));
            DateTime fakeNow = marketEndTime;

            // Mock DateTime.Now
            MockDateTime(fakeNow);

            // Act
            var task = _tradingEngine.ProcessAtMarketEnd();
            _cts.Cancel(); // Simulate cancellation to exit the loop

            // Assert
            _mockOrderbook.Verify(o => o.DeleteGoodForDayOrders(), Times.Once);
            _mockOrderbook.Verify(o => o.DeleteExpiredGoodTillCancel(), Times.Once);
            _mockHiddenOrderbook.Verify(h => h.DeleteGoodForDayOrders(), Times.Once);
            _mockHiddenOrderbook.Verify(h => h.DeleteExpiredGoodTillCancel(), Times.Once);
            mockOrder.Verify(o => o.Dispose(), Times.Once);
        }

        [Test]
        public async Task ProcessAtMarketOpen_ShouldMatchOrdersAndDispose()
        {
            // Arrange
            var mockOrder = new Mock<Order>();
            mockOrder.Setup(o => o.OrderID).Returns(1);
            _tradingEngine._onMarketOpen.Add(1, mockOrder.Object);

            DateTime marketOpenTime = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            DateTime fakeNow = marketOpenTime;

            // Mock DateTime.Now
            MockDateTime(fakeNow);

            // Act
            var task = _tradingEngine.ProcessAtMarketOpen();
            _cts.Cancel(); // Simulate cancellation to exit the loop

            // Assert
            _mockMatchingEngine.Verify(m => m.match(It.IsAny<Order>()), Times.Once);
            mockOrder.Verify(o => o.Dispose(), Times.Once);
        }

        [Test]
        public void ProcessOnMarketEndOrders_ShouldMatchAndDisposeOrders()
        {
            // Arrange
            var mockOrder = new Mock<Order>();
            mockOrder.Setup(o => o.OrderID).Returns(1);
            _tradingEngine._onMarketClose.Add(1, mockOrder.Object);

            // Act
            _tradingEngine.ProcessOnMarketEndOrders();

            // Assert
            _mockMatchingEngine.Verify(m => m.match(It.IsAny<Order>()), Times.Once);
            mockOrder.Verify(o => o.Dispose(), Times.Once);
        }

        private void MockDateTime(DateTime fakeNow)
        {
            // Mock DateTime.Now using a helper or library like JustMock or similar
            // This is a placeholder for mocking DateTime.Now
        }
    }
    */
}