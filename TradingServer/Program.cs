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

TradingOrderbookPriceMethodsTest priceMethodsTest = new TradingOrderbookPriceMethodsTest();

priceMethodsTest.Setup();
priceMethodsTest.TestPriceUpdatedCorrectly();

TradingOrderbookMatchingTest matchingTest = new TradingOrderbookMatchingTest();

matchingTest.Setup();
matchingTest.PostOnlyMatchTest();
matchingTest.ImmediateHandleTypeMatchedTest();
matchingTest.HiddenAndVisibleOrdersMatchedCorrectly();

TradingOrderbookStopOrderTest stopOrderTest = new TradingOrderbookStopOrderTest();

stopOrderTest.Setup();
stopOrderTest.StopOrderAddedWhenLatestPriceChanges();

/*
TradingOrderbookIcebergOrdersTest icebergOrdersTest = new TradingOrderbookIcebergOrdersTest();
icebergOrdersTest.Setup();
await icebergOrdersTest.IcebergTest();
*/

Console.WriteLine("Starting trading server...\n");

_ = Task.Run(() => processInputs());
using var engine = TradingHostBuilder.BuildTradingServer();
await engine.StartAsync().ConfigureAwait(false);

static void processInputs()
{
    // A starting point for any API for this trading platform.
}




