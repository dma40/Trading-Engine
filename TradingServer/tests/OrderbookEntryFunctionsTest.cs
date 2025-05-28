using NUnit.Framework;
using TradingServer.OrderbookCS;
using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.Tests
{
    [TestFixture]
    public class OrderbookEntryFunctionsTest
    {
        private Orderbook testOrderbook;

        [SetUp]
        public void Setup()
        {
            testOrderbook = new Orderbook(new Security("TEST", "TEST_ID"));
        }

        [Test]
        public void AddRemoveOrderTest()
        {
            List<Order> testOrders = new List<Order>();
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, (uint)i, false));
            }

            foreach (Order order in testOrders)
            {
                testOrderbook.addOrder(order);
            }

            Assert.That(5000 == testOrderbook.getAskLimits().Count);

            int j = 0;

            foreach (Limit limit in testOrderbook.getAskLimits())
            {
                Assert.That(limit.Price == 4999 - j);
                Assert.That(4 * (4999 - j) + 3 == limit?.tail?.CurrentOrder.OrderID);
                Assert.That(4 * (4999 - j) + 2 == limit?.tail?.previous?.CurrentOrder.OrderID);
                Assert.That(4 * (4999 - j) + 1 == limit?.tail?.previous?.previous?.CurrentOrder.OrderID);
                Assert.That(4 * (4999 - j) == limit?.tail?.previous?.previous?.previous?.CurrentOrder.OrderID);
                j++;
            }

            for (int i = 0; i < 5000; i++)
            {
                CancelOrder first_cancel = testOrders[4 * i + 2].cancelOrder();
                testOrderbook.removeOrder(first_cancel);

                CancelOrder second_cancel = testOrders[4 * i + 1].cancelOrder();
                testOrderbook.removeOrder(second_cancel);

                CancelOrder third_cancel = testOrders[4 * i + 3].cancelOrder();
                testOrderbook.removeOrder(third_cancel);

                CancelOrder fourth_cancel = testOrders[4 * i].cancelOrder();
                testOrderbook.removeOrder(fourth_cancel);
            }

            Assert.That(testOrderbook.getAskOrders().Count == 0);
            Assert.That(testOrderbook.getAskLimits().Count == 0);
        }

        [Test]
        public void ModifyOrderTest()
        {
            List<Order> testOrders = new List<Order>();
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, (uint)i, false));
            }

            foreach (Order order in testOrders)
            {
                testOrderbook.addOrder(order);
            }

            for (int i = 0; i < 20000; i++)
            {
                var order = testOrders[i];
                ModifyOrder modify = new ModifyOrder(order, order.Price + 5000, order.Quantity, order.isBuySide);
                testOrderbook.modifyOrder(modify);
            }

            Assert.That(testOrderbook.getAskLimits().Count == 5000);

            int j = 9999;
            foreach (Limit limit in testOrderbook.getAskLimits())
            {
                Assert.That(limit?.Price == j);
                Assert.That(limit?.head?.CurrentOrder.OrderID == 4 * (j - 5000));
                Assert.That(limit?.head?.next?.CurrentOrder.OrderID == 4 * (j - 5000) + 1);
                Assert.That(limit?.head?.next?.next?.CurrentOrder.OrderID == 4 * (j - 5000) + 2);
                Assert.That(limit?.head?.next?.next?.next?.CurrentOrder.OrderID == 4 * (j - 5000) + 3);
                j--;
            }
        }
    }
}