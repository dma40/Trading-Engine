FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /TradingEngine

COPY TradingServer.sln

COPY Logging/Logging.csproj ./Logging/
COPY OrderHandlers/OrderHandlers/csproj ./OrderHandlers/
COPY TradingServer/TradingServer.csproj ./TradingServer/

COPY Orderbook/Orders/Orders.csproj ./Orders/
COPY Orderbook/OrderbookCS/OrderbookCS.csproj ./OrderbookCS/

COPY TradingServices/TradingServices.csproj ./TradingServices/

RUN dotnet restore TradingServer.sln

COPY . .

RUN dotnet publish TradingServer.sln -c Release -o /TradingEngine/publish

EXPOSE 12000

# next steps:
# release the project, then add the appropriate copy statement here

COPY --from=build /TradingEngine/publish

# verify that this file works right
ENTRYPOINT ["dotnet", "TradingServer.dll"]