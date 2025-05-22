using System.Collections.Generic;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using Xunit;
using TradingServer.Instrument;

namespace TradingServer.Tests
{
    public class TradingOrderbookMatchingTest
    {
        private TradingEngine _tradingEngine;

        [Fact]
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
    }
    /*
    public class TradingOrderbookMatchingTest
    {
        private readonly Mock<Orderbook> _mockVisibleOrderbook;
        private readonly Mock<Orderbook> _mockHiddenOrderbook;
        private readonly Mock<Trades> _mockTrades;
        private readonly TradingEngine _tradingEngine;

        public TradingOrderbookMatchingTest()
        {
            _mockVisibleOrderbook = new Mock<Orderbook>();
            _mockHiddenOrderbook = new Mock<Orderbook>();
            _mockTrades = new Mock<ITrades>();

            _tradingEngine = new TradingEngine(_mockOrderbook.Object, _mockHiddenOrderbook.Object, _mockTrades.Object);
        }

        [Fact]
        public void Match_ImmediateHandleType_OrderMatched()
        {
            // Arrange
            var order = new Order { OrderType = (OrderTypes)3 };
            var trades = new Trades();
            _mockOrderbook.Setup(o => o.match(order)).Returns(trades);
            _mockHiddenOrderbook.Setup(h => h.match(order)).Returns(new Trades());

            // Act
            var result = _tradingEngine.match(order);

            // Assert
            _mockOrderbook.Verify(o => o.match(order), Times.Once);
            _mockHiddenOrderbook.Verify(h => h.match(order), Times.Once);
            _mockTrades.Verify(t => t.addTransactions(It.IsAny<Trades>()), Times.Once);
        }

        [Fact]
        public void Match_FillOrKill_EligibleOrderCount_OrderMatched()
        {
            // Arrange
            var order = new Order { OrderType = OrderTypes.FillOrKill, CurrentQuantity = 10 };
            _mockOrderbook.Setup(o => o.getEligibleOrderCount(order)).Returns(5);
            _mockHiddenOrderbook.Setup(h => h.getEligibleOrderCount(order)).Returns(6);
            _mockOrderbook.Setup(o => o.match(order)).Returns(new Trades());
            _mockHiddenOrderbook.Setup(h => h.match(order)).Returns(new Trades());

            // Act
            var result = _tradingEngine.match(order);

            // Assert
            _mockOrderbook.Verify(o => o.match(order), Times.Once);
            _mockHiddenOrderbook.Verify(h => h.match(order), Times.Once);
        }

        [Fact]
        public void Match_PostOnly_OrderNotMatched_AddedToOrderbook()
        {
            // Arrange
            var order = new Order { OrderType = OrderTypes.PostOnly };
            _mockOrderbook.Setup(o => o.canMatch(order)).Returns(false);

            // Act
            _tradingEngine.match(order);

            // Assert
            _mockOrderbook.Verify(o => o.addOrder(order), Times.Once);
        }

        [Fact]
        public void Match_Default_OrderMatchedAndAddedToOrderbook()
        {
            // Arrange
            var order = new Order { OrderType = OrderTypes.Limit, CurrentQuantity = 10, isHidden = false };
            _mockOrderbook.Setup(o => o.match(order)).Returns(new Trades());
            _mockHiddenOrderbook.Setup(h => h.match(order)).Returns(new Trades());

            // Act
            _tradingEngine.match(order);

            // Assert
            _mockOrderbook.Verify(o => o.match(order), Times.Once);
            _mockHiddenOrderbook.Verify(h => h.match(order), Times.Once);
            _mockOrderbook.Verify(o => o.addOrder(order), Times.Once);
        }

        [Fact]
        public void Match_HiddenOrder_OrderAddedToHiddenOrderbook()
        {
            // Arrange
            var order = new Order { OrderType = OrderTypes.Limit, CurrentQuantity = 10, isHidden = true };
            _mockOrderbook.Setup(o => o.match(order)).Returns(new Trades());
            _mockHiddenOrderbook.Setup(h => h.match(order)).Returns(new Trades());

            // Act
            _tradingEngine.match(order);

            // Assert
            _mockHiddenOrderbook.Verify(h => h.addOrder(order), Times.Once);
        }

        [Fact]
        public void HasEligibleOrderCount_ReturnsTrue_WhenEligibleOrdersExist()
        {
            // Arrange
            var order = new Order { CurrentQuantity = 10 };
            _mockOrderbook.Setup(o => o.getEligibleOrderCount(order)).Returns(5);
            _mockHiddenOrderbook.Setup(h => h.getEligibleOrderCount(order)).Returns(6);

            // Act
            var result = _tradingEngine.hasEligibleOrderCount(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasEligibleOrderCount_ReturnsFalse_WhenNoEligibleOrdersExist()
        {
            // Arrange
            var order = new Order { CurrentQuantity = 10 };
            _mockOrderbook.Setup(o => o.getEligibleOrderCount(order)).Returns(3);
            _mockHiddenOrderbook.Setup(h => h.getEligibleOrderCount(order)).Returns(2);

            // Act
            var result = _tradingEngine.hasEligibleOrderCount(order);

            // Assert
            Assert.False(result);
        }
    }
    */
}