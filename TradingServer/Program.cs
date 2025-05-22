using TradingServer.Tests;
using TradingServer.Core;

OrderbookEntryFunctionsTest orderbookEntryFunctionsTest = new OrderbookEntryFunctionsTest();

orderbookEntryFunctionsTest.Setup();
orderbookEntryFunctionsTest.AddRemoveOrderTest();
orderbookEntryFunctionsTest.ModifyOrderTest();


OrderbookMatchingTest orderbookMatchingTest = new OrderbookMatchingTest();
orderbookMatchingTest.Setup();
orderbookMatchingTest.canFillTests();
orderbookMatchingTest.MatchTest();
orderbookMatchingTest.getEligibleOrderCountTest();

TradingOrderbookIcebergOrdersTest icebergOrdersTest = new TradingOrderbookIcebergOrdersTest();
icebergOrdersTest.Setup();
await icebergOrdersTest.IcebergTest();

Console.WriteLine("Starting trading server...\n");

_ = Task.Run(() => processInputs());
using var engine = TradingHostBuilder.BuildTradingServer();
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any user interface for this trading platform.
}




