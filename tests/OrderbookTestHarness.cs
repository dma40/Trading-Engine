using System.Security.Cryptography.X509Certificates;
using TradingServer.OrderbookCS;
using TradingServer.Orders;

using TradingServer.Instrument;

namespace TradingServer.Tests
{
    public sealed class OrderbookTestHarness
    {
        // just check each of the limits, and then check if each of the orders has the same reference - this should do the trick
        private static void addSingleOrders()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));

            IOrderCore orderCore1 = new OrderCore(100, "Dylan", "037833100", OrderTypes.GoodTillCancel);
            ModifyOrder modify1 = new ModifyOrder(orderCore1, 50, 100, true);

            Order order1 = modify1.newOrder();

            IOrderCore orderCore2 = new OrderCore(200, "Dylan", "037833100", OrderTypes.GoodForDay);
            ModifyOrder modify2 = new ModifyOrder(orderCore2, 50, 100, false);

            Order order2 = modify2.newOrder();

            orders.addOrder(order1);
            orders.addOrder(order2); // add method to get ids, check if the IDs are correct and/or references correct

            Console.WriteLine("ADD SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("ADD SINGLE ORDER TEST B: " + false + '\n');
            Console.WriteLine("ADD SINGLE ORDER TEST C" + true + "\n");
        }

        private static void removeSingleOrders()
        {
            Orderbook orders = new Orderbook(new Security("AAPL"));

            Console.WriteLine("REMOVE SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("REMOVE SINGLE ORDER TEST B: " + false + '\n');
        }

        private static void modifySingleOrders()
        {
            Console.WriteLine("MODIFY SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("MODIFY SINGLE ORDER TEST B: " + false + '\n');
            
        }

        private static void removeNearEnds()
        {

        }

        private static void modifyNearEnds()
        {

        }

        private static void removeNearMiddle()
        {

        }

        private static void modifyNearMiddle()
        {

        }

        private static void testMatchBasic()
        {

        }

        private static void testMatchWithResting()
        {

        }

        private static void testMatchWithFillOrKill()
        {

        }

        private static void testMatchWithFillAndKill()
        {

        }

        private static void testMatchWithMarket()
        {

        }

        private static void testMatchAndRemove()
        {

        }

        private static void testMatchAndAdd()
        {

        }

        public static void runTests()
        {
            addSingleOrders();
            removeSingleOrders();
            modifySingleOrders();

            removeNearMiddle();
            modifyNearMiddle();

            removeNearEnds();
            modifyNearEnds();

            testMatchBasic();
            testMatchWithResting();
            testMatchWithFillAndKill();
            testMatchWithFillOrKill();
            testMatchWithMarket();
            testMatchAndAdd();
            testMatchAndRemove();
        }
    }
}


