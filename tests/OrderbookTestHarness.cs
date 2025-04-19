using System.Security.Cryptography.X509Certificates;
using TradingServer.OrderbookCS;
using TradingServer.Orders;

using TradingServer.Instrument;

namespace TradingServer.Tests
{
    public class OrderbookTestHarness
    {
        private void addSingleOrders()
        {
            FIFOrderbook orders = new FIFOrderbook(new Security("AAPL"));

            IOrderCore orderCore1 = new OrderCore(100, "Dylan", "037833100", OrderTypes.GoodTillCancel);
            ModifyOrder modify1 = new ModifyOrder(orderCore1, 50, 100, true);

            IOrderCore orderCore2 = new OrderCore(200, "Dylan", "037833100", OrderTypes.GoodForDay);
            ModifyOrder modify2 = new ModifyOrder(orderCore2, 50, 100, false);

            orders.addOrder(modify1.newOrder());
            orders.addOrder(modify2.newOrder());

            Console.WriteLine("ADD SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("ADD SINGLE ORDER TEST B: " + false + '\n');
        }

        private void removeSingleOrders()
        {
            Orderbook orders = new FIFOrderbook(new Security("AAPL"));

            Console.WriteLine("REMOVE SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("REMOVE SINGLE ORDER TEST B: " + false + '\n');
        }

        private void modifySingleOrders()
        {
            Console.WriteLine("MODIFY SINGLE ORDER TEST A: " + true + '\n');
            Console.WriteLine("MODIFY SINGLE ORDER TEST B: " + false + '\n');
            
        }

        private void addNearEnds()
        {
            Console.WriteLine("ADD NEAR START TEST A: " + true + '\n');
            Console.WriteLine("ADD NEAR END TEST B: " + false + '\n');
            Console.WriteLine("ADD NEAR START TEST C: " + true + '\n');
            Console.WriteLine("ADD NEAR END TEST D: " + false + '\n');
        }

        private void removeNearEnds()
        {

        }

        private void modifyNearEnds()
        {

        }

        private void addInMiddle()
        {

        }

        private void removeNearMiddle()
        {

        }

        private void modifyNearMiddle()
        {

        }

        private void testMatchBasic()
        {

        }

        private void testMatchWithResting()
        {

        }

        private void testMatchWithFillOrKill()
        {

        }

        private void testMatchWithFillAndKill()
        {

        }

        private void testMatchwithMarket()
        {

        }

        private void testMatchAndRemove()
        {

        }

        private void testMatchAndAdd()
        {

        }

        public void runAllTests()
        {
            addSingleOrders();
            removeSingleOrders();
            modifySingleOrders();

            addNearEnds();
            removeNearEnds();
            modifyNearEnds();

            testMatchBasic();
            testMatchWithFillAndKill();
            testMatchWithFillOrKill();
            testMatchwithMarket();
            testMatchAndAdd();
            testMatchAndRemove();
        }
    }
}


