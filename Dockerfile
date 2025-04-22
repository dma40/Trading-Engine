FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /Trading-Engine

COPY TradingServer.sln .

COPY Logging/ ./Logging/
COPY OrderHandlers/ ./OrderHandlers/
COPY TradingServer/ ./TradingServer/

#COPY Orderbook/Orders/Orders.csproj ./Orders/
#COPY Orderbook/OrderbookCS/OrderbookCS.csproj ./OrderbookCS/

COPY Orderbook/ ./Orderbook/

COPY TradingServices/TradingServices.csproj ./TradingServices/

RUN dotnet restore TradingServer.sln

COPY . .

RUN mkdir -p /Trading-Engine/publish && chmod -R 777 /Trading-Engine/publish
RUN dotnet publish TradingServer.sln -c Release -o /Trading-Engine/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /Trading-Engine
EXPOSE 12000

COPY --from=build /Trading-Engine/publish .

ENTRYPOINT ["dotnet", "TradingServer.dll"]