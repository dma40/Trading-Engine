using System.Collections.Generic;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using TradingServer.Instrument;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookMatchingTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public async Task FillOrKillTest()
        {
            List<Order> testOrders = new List<Order>();

            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, 1, false));
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }

            IOrderCore unfillableFOKCore = new OrderCore(20000, "Dylan", "TEST", OrderTypes.FillOrKill);
            Order unfillableFOKOrder = new Order(unfillableFOKCore, 100, 100000000, true);

            await _tradingEngine.addOrder(unfillableFOKOrder);

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 5000);

            IOrderCore fillableFOKCore = new OrderCore(20001, "Dylan", "TEST", OrderTypes.FillOrKill);
            Order fillableFOKOrder = new Order(fillableFOKCore, 100, 404, true);

            await _tradingEngine.addOrder(fillableFOKOrder);

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 4899);

            for (int i = 404; i < 20000; i++)
            {
                await _tradingEngine.removeOrder(testOrders[i].cancelOrder());
            }

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 0);
        }

        [Test]
        public async Task ImmediateHandleTypeMatchedTest()
        {
            List<Order> testOrders = new List<Order>();

            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, 1, false));
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }

            IOrderCore fakCore = new OrderCore(20000, "Dylan", "TEST", OrderTypes.FillAndKill);
            Order fakOrder = new Order(fakCore, 100, 1000000, true);

            await _tradingEngine.addOrder(fakOrder);

            Assert.That(!_tradingEngine.containsOrder(fakOrder.OrderID));
            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 4899);

            for (int i = 404; i < 20000; i++)
            {
                await _tradingEngine.removeOrder(testOrders[i].cancelOrder());
            }

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 0);
            Assert.That(_tradingEngine.orderbook.getBidLimits().Count == 0);
        }

        [Test]
        public async Task PostOnlyMatchTest()
        {
            List<Order> testOrders = new List<Order>();

            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                testOrders.Add(new Order(core, i / 4, 1, true));
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, true));
            }

            IOrderCore unmatchableCoreSameSide = new OrderCore(20000, "Dylan", "TEST", OrderTypes.PostOnly);
            Order unmatchableSameSideOrder = new Order(unmatchableCoreSameSide, 5000, 1, true);

            await _tradingEngine.addOrder(unmatchableSameSideOrder);
            Assert.That(_tradingEngine.containsOrder(unmatchableSameSideOrder.OrderID));

            IOrderCore unmatchableCore = new OrderCore(20001, "Dylan", "TEST", OrderTypes.PostOnly);
            Order unmatchableOrder = new Order(unmatchableCore, 5001, 5001, false);

            await _tradingEngine.addOrder(unmatchableOrder);

            Assert.That(_tradingEngine.containsOrder(unmatchableOrder.OrderID));

            IOrderCore matchableCore = new OrderCore(20002, "Dylan", "TEST", OrderTypes.PostOnly);
            Order matchableOrder = new Order(matchableCore, 100, 100000, false);

            await _tradingEngine.addOrder(matchableOrder);
            Assert.That(!_tradingEngine.containsOrder(matchableOrder.OrderID));

            await _tradingEngine.removeOrder(unmatchableOrder.cancelOrder());
            await _tradingEngine.removeOrder(unmatchableSameSideOrder.cancelOrder());

            foreach (Order order in testOrders)
            {
                await _tradingEngine.removeOrder(order.cancelOrder());
            }

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 0);
            Assert.That(_tradingEngine.orderbook.getBidLimits().Count == 0);
        }

        [Test]
        public async Task HiddenAndVisibleOrdersMatchedCorrectly()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel, true);
                IOrderCore visibleCore = new OrderCore(i + 20000, "Dylan", "TEST", OrderTypes.GoodTillCancel);

                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
                await _tradingEngine.addOrder(new Order(visibleCore, i / 4, 1, false));

                Assert.That(!_tradingEngine.orderbook.containsOrder(i));
            }

            IOrderCore visible = new OrderCore(40000, "Dylan", "TEST", OrderTypes.GoodTillCancel);
            Order visibleOrder = new Order(visible, 2, 24, true);

            await _tradingEngine.addOrder(visibleOrder);

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 4997);
            Assert.That(!_tradingEngine.containsOrder(visibleOrder.OrderID));

            IOrderCore hidden = new OrderCore(40001, "Dylan", "TEST", OrderTypes.GoodTillCancel, true);
            Order hiddenOrder = new Order(hidden, 3, 12, true);

            await _tradingEngine.addOrder(hiddenOrder);

            Assert.That(_tradingEngine.orderbook.getAskLimits().Count == 4996);
            Assert.That(hiddenOrder.CurrentQuantity == 4);
        }
    }
}