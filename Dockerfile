FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /Trading-Engine

RUN apt-get update && apt-get install -y protobuf-compiler 

COPY TradingServer.sln .

COPY Logging/ ./Logging/
COPY TradingServer/ ./TradingServer/
COPY Orderbook/ ./Orderbook/
COPY TradingServices/ ./TradingServices/

RUN dotnet restore TradingServer.sln

RUN dotnet build -c Release

RUN mkdir -p /Trading-Engine/publish && chmod -R 777 /Trading-Engine/publish
RUN dotnet publish TradingServer.sln -c Release 

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /Trading-Engine
EXPOSE 12000

COPY --from=build /Trading-Engine/publish .

ENTRYPOINT ["dotnet", "TradingServer.dll"]