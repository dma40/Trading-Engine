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
            Orderbook orders = new Orderbook(new Security("AAPL"));
        }

        private void removeSingleOrders()
        {
            Orderbook orders = new Orderbook(new Security("AAPL"));
        }

        private void modifySingleOrders()
        {

        }

        private void addNearEnds()
        {

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


