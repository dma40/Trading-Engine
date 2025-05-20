cmake_minimum_required(VERSION 3.20)
project(TradingServer)

file(GLOB_RECURSE ORDERBOOK_SOURCES "Orderbook/OrderbookCS/*.cs")
file(GLOB_RECURSE ORDER_SOURCES "Orderbook/Orders/*.cs")

file(GLOB_RECURSE LOGGING_SOURCES "Logging/*.cs")

file(GLOB_RECURSE SERVER_SOURCES "TradingServer/*.cs")

file(GLOB_RECURSE SERVICE_SOURCES "TradingServices/*.cs")

add_custom_command(
        OUTPUT Orderbook/Orders/bin/Debug/net9.0/TradingServer.Orders.dll
        COMMAND dotnet build Orderbook/Orders/Orders.csproj
        DEPENDS ${ORDERBOOK_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        Orders
        DEPENDS Orderbook/Orders/bin/Debug/net9.0/TradingServer.Orders.dll
)

add_custom_command(
        OUTPUT Orderbook/OrderbookCS/bin/Debug/net9.0/TradingServer.Orderbook.dll
        COMMAND dotnet build Orderbook/OrderbookCS/OrderbookCS.csproj
        DEPENDS ${ORDERBOOK_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        OrderbookCS
        DEPENDS Orderbook/OrderbookCS/bin/Debug/net9.0/TradingServer.Orderbook.dll;
)

add_custom_command(
        OUTPUT Logging/bin/Debug/net9.0/TradingServer.Logging.dll
        COMMAND dotnet build Logging/Logging.csproj
        DEPENDS ${LOGGING_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        Logging
        DEPENDS Logging/bin/debug/net9.0/TradingServer.Logging.dll
)

add_custom_command(
        OUTPUT TradingServices/bin/Debug/net9.0/TradingServer.Services.dll
        COMMAND dotnet build TradingServices/TradingServices.csproj
        DEPENDS ${SERVICE_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        TradingServices
        DEPENDS TradingServices/bin/debug/net9.0/TradingServer.Services.dll
)

add_custom_command(
        OUTPUT TradingServer/bin/Debug/net9.0/TradingServer.Core.dll
        COMMAND dotnet build TradingServer/TradingServer.csproj
        DEPENDS ${SERVER_SOURCES} OrderbookCS Logging TradingServices
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        TradingServer
        DEPENDS TradingServer/bin/debug/net9.0/TradingServer.Core.dll
)

add_custom_target(
        Restore ALL
        COMMAND dotnet restore TradingServer/TradingServer.csproj
)
add_dependencies(TradingServer Restore)

add_custom_target(BackgroundTradingHost ALL
        COMMAND dotnet run --project TradingServer/TradingServer.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        COMMENT "Starting trading server..."
)

