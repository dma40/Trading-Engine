﻿using TradingServer.Tests;
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

TradingOrderbookMatchingTest matchingTest = new TradingOrderbookMatchingTest();

matchingTest.Setup();

matchingTest.FillOrKillTest();
matchingTest.PostOnlyMatchTest();
matchingTest.ImmediateHandleTypeMatchedTest();
matchingTest.HiddenAndVisibleOrdersMatchedCorrectly(); 

var watch = System.Diagnostics.Stopwatch.StartNew();

TradingOrderbookPriceMethodsTest priceMethodsTest = new TradingOrderbookPriceMethodsTest();

priceMethodsTest.Setup();
priceMethodsTest.TestPriceUpdatedCorrectly();

watch.Stop();


// Console.WriteLine("Time to execute tests: " + watch.ElapsedMilliseconds);

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
using var server = TradingHostBuilder.BuildTradingServer();
await server.StartAsync().ConfigureAwait(false);

/*
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




