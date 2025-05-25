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
await priceMethodsTest.TestPriceUpdatedCorrectly();

TradingOrderbookMatchingTest matchingTest = new TradingOrderbookMatchingTest();

matchingTest.Setup();
await matchingTest.PostOnlyMatchTest();
await matchingTest.ImmediateHandleTypeMatchedTest();
await matchingTest.HiddenAndVisibleOrdersMatchedCorrectly();

TradingOrderbookStopOrderTest stopOrderTest = new TradingOrderbookStopOrderTest();

stopOrderTest.Setup();
await stopOrderTest.StopOrderAddedWhenLatestPriceChanges();

/*
TradingOrderbookIcebergOrdersTest icebergOrdersTest = new TradingOrderbookIcebergOrdersTest();
icebergOrdersTest.Setup();
await icebergOrdersTest.IcebergTest();
*/

Console.WriteLine("Starting trading server...\n");

_ = Task.Run(() => processInputs());
using var server = TradingHostBuilder.BuildTradingServer();
await server.StartAsync().ConfigureAwait(false);

/* Alternatively,

TradingServerServiceProvider.ServiceProvider = server.Services;
{
    using var scope = TradingServerServiceProvider.ServiceProvider.CreateScope();
    await server.RunAsync().ConfigureAwait(false);
}

*/

static void processInputs()
{
    // A starting point for any API for this trading platform.
}




