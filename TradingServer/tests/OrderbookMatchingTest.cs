using NUnit.Framework;
using TradingServer.OrderbookCS;
using TradingServer.Orders;
using TradingServer.Instrument;
using Xunit.Sdk;

namespace TradingServer.Tests
{
    [TestFixture]
    public class OrderbookMatchingTest
    {
        private Orderbook _orderbook;

        [SetUp]
        public void Setup()
        {
            _orderbook = new Orderbook(new Security("TEST", "TEST_ID"));
        }

        [Test]
        public void getEligibleOrderCountTest()
        {
            List<Order> testOrders = new List<Order>();
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, 1, false));
            }

            foreach (Order order in testOrders)
            {
                _orderbook.addOrder(order);
            }

            IOrderCore testCore = new OrderCore(20001, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order testOrder = new Order(testCore, 500, 100, true);

            Assert.That(_orderbook.canFill(testOrder));
            Assert.That(_orderbook.getEligibleOrderCount(testOrder) == 18000);

            for (int i = 0; i < 20000; i++)
            {
                CancelOrder cancel = testOrders[i].cancelOrder();
                _orderbook.removeOrder(cancel);
            }
        }

        [Test]
        public void canFillTests()
        {
            List<Order> testOrders = new List<Order>();
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, (uint)i, false));
            }

            foreach (Order order in testOrders)
            {
                _orderbook.addOrder(order);
            }

            IOrderCore sameSideTestFillable = new OrderCore(20001, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order sameSideTestFillableOrder = new Order(sameSideTestFillable, 200, 5000, false);

            Assert.That(!_orderbook.canFill(sameSideTestFillableOrder));

            IOrderCore opposingSideFillable = new OrderCore(20002, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order opposingSideFillableOrder = new Order(opposingSideFillable, 200, 5000, true);

            Assert.That(_orderbook.canFill(opposingSideFillableOrder));

            for (int i = 0; i < 20000; i++)
            {
                CancelOrder cancel = testOrders[i].cancelOrder();
                _orderbook.removeOrder(cancel);
            }
        }

        [Test]
        public void MatchTest()
        {
            List<Order> testOrders = new List<Order>();
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, 1, true));
            }

            foreach (Order order in testOrders)
            {
                _orderbook.addOrder(order);
            }

            IOrderCore opposingSideMatchable = new OrderCore(20002, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order opposingSideMatchableOrder = new Order(opposingSideMatchable, 200, 5000, false);

            _orderbook.match(opposingSideMatchableOrder);
      
            Assert.That(opposingSideMatchableOrder.CurrentQuantity == 4196);
            Assert.That(_orderbook.getBidLimits().Count == 4799);

            IOrderCore currentQuantityMoreThanVolume = new OrderCore(20003, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order currentQuantityMoreThanVolumeOrder = new Order(currentQuantityMoreThanVolume, 10000, 19197, false);

            _orderbook.match(currentQuantityMoreThanVolumeOrder);
            Assert.That(_orderbook.getBidLimits().Count == 0);
            Assert.That(currentQuantityMoreThanVolumeOrder.CurrentQuantity == 1);

            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                _orderbook.addOrder(new Order(core, i / 4, 1, true));
            }

            IOrderCore currentQuantityCanBeFullyFilled = new OrderCore(20004, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order currentQuantityCanBeFullyFilledTest = new Order(currentQuantityCanBeFullyFilled, 10000, 19999, false);

            _orderbook.match(currentQuantityCanBeFullyFilledTest);
            Assert.That(_orderbook.getBidLimits().Count == 1);
            Assert.That(currentQuantityCanBeFullyFilledTest.CurrentQuantity == 0);

            _orderbook.removeOrder(testOrders[19999].cancelOrder()); 
        }
    }
}