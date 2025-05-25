using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using NUnit.Framework;
using TradingServer.Instrument;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace TradingServer.Tests
{
    [TestFixture]
    public class TradingOrderbookIcebergOrdersTest
    {
        private TradingEngine _tradingEngine;

        [SetUp]
        public void Setup()
        {
            _tradingEngine = new TradingEngine(new Security("TEST"));
        }

        [Test]
        public async Task IcebergTest()
        {
            for (int i = 0; i < 20000; i++)
            {
                IOrderCore core = new OrderCore(i, "Dylan", "TEST", OrderTypes.GoodTillCancel);
                await _tradingEngine.addOrder(new Order(core, i / 4, 1, false));
            }

            OrderCore icebergCore = new OrderCore(20000, "Dylan", "TEST", OrderTypes.Iceberg);
            IcebergOrder icebergTest = new IcebergOrder(icebergCore, 1000000000, 2, true, 2);

            await _tradingEngine.addOrder(icebergTest);
            await Task.Delay(100);

            Console.WriteLine(icebergTest.CurrentQuantity);

            //Assert.That(_tradingEngine.containsOrder(icebergTest.OrderID));
        }
    }
}