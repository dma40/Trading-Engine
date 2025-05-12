using TradingServer.OrderbookCS;
using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.Tests
{
    internal static class OrderbookUnitTests
    {
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
            orders.addOrder(order2); 

            bool test_one_success = getBidOrderIds(orders).SequenceEqual(new List<long> {  });
            bool test_two_success = getAskOrderIds(orders).SequenceEqual(new List<long> {  });

            Console.WriteLine("ADD SINGLE ORDER TEST A: " + test_one_success + '\n');
            Console.WriteLine("ADD SINGLE ORDER TEST B: " + test_two_success + '\n');
            Console.WriteLine("Bid order ids: ");
            getBidOrderIds(orders);
            Console.WriteLine('\n');
            Console.WriteLine("Ask order ids: ");
            getAskOrderIds(orders);

            Console.WriteLine(orders.getAskOrders().Count == 0);
            Console.WriteLine(orders.getBidOrders().Count == 0);
            Console.WriteLine("Ask limits: \n");
            getAskLimits(orders);
            Console.WriteLine("Bid limits: \n");
            getBidLimits(orders);

            IOrderCore orderCore3 = new OrderCore(300, "Dylan", "037833100", OrderTypes.GoodTillCancel);
            ModifyOrder modify3 = new ModifyOrder(orderCore3, 50, 100, true);

            Order order3 = modify3.newOrder();
            orders.addOrder(order3);

            Console.WriteLine("After adding a new order:");
            getBidLimits(orders);

            CancelOrder cancel3 = modify3.cancelOrder();
            orders.removeOrder(cancel3);

            Console.WriteLine("After removing one order:");
            getBidLimits(orders);
        }

        private static void removeSingleOrders()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));

            /*
            Console.WriteLine("REMOVE SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("REMOVE SINGLE ORDER TEST B: " + false + '\n');
            */
        }

        private static void modifySingleOrders()
        {
            /*
            Console.WriteLine("MODIFY SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("MODIFY SINGLE ORDER TEST B: " + false + '\n');
            */
        }

        private static void removeNearEnds()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void modifyNearEnds()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void removeNearMiddle()
        {
           Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void modifyNearMiddle()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchBasic()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchWithResting()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchWithFillOrKill()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchWithFillAndKill()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchWithMarket()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchAndRemove()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static void testMatchAndAdd()
        {
            Orderbook orders = new Orderbook(new Security("test_security"));
        }

        private static List<long> getBidOrderIds(IRetrievalOrderbook orderbook)
        {
            List<long> result = new List<long>();
            foreach (OrderbookEntry obe in orderbook.getBidOrders())
            {
                result.Add(obe.CurrentOrder.OrderID);
                Console.WriteLine(obe.CurrentOrder.OrderID + " ");
            }
            return result;
        }

        private static List<long> getAskOrderIds(IRetrievalOrderbook orderbook)
        {
            List<long> result = new List<long>();
            foreach (OrderbookEntry obe in orderbook.getAskOrders())
            {    
                result.Add(obe.CurrentOrder.OrderID);
                Console.WriteLine(obe.CurrentOrder.OrderID + " ");
            }
            return result;
        }

        private static void getAskLimits(Orderbook orderbook)
        {
            foreach (Limit limit in orderbook.getAskLimits())
            {
                Console.WriteLine(limit.Price + "\n");
                if (limit.head == null)
                {
                    Console.WriteLine("head is null");
                }

                else
                {
                    Console.WriteLine("head is not null ");
                    Console.WriteLine(limit.head.CurrentOrder.OrderID);

                    if (limit.head.next != null)
                    {
                        Console.WriteLine("head's next is not null");
                        Console.WriteLine(limit.head.next.CurrentOrder.OrderID);
                        Console.WriteLine(limit.head.next == limit.tail);
                    } /* Also print whether it's the head, it'll help with debugging remove orders method */
                }
            }
        }

        private static void getBidLimits(Orderbook orderbook)
        {
            foreach (Limit limit in orderbook.getBidLimits())
            {
                Console.WriteLine(limit.Price + "\n");
                if (limit.head == null)
                    Console.WriteLine("head is null ");

                else
                {
                    Console.WriteLine("head is not null ");
                    Console.WriteLine(limit.head.CurrentOrder.OrderID);

                    if (limit.head.next != null)
                    {
                        Console.WriteLine("head's next is not null");
                        Console.WriteLine(limit.head.next.CurrentOrder.OrderID);
                        Console.WriteLine(limit.head.next == limit.tail);
                    }
                }
            }
        }

        public static void runTests()
        {
            Console.WriteLine("Unit test results: \n");

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


