cmake_minimum_required(VERSION 3.20)
project(Trading-Engine)

file(GLOB_RECURSE ORDERBOOK_SOURCES "Orderbook/OrderbookCS/*.cs")
file(GLOB_RECURSE ORDER_SOURCES "Orderbook/Orders/*.cs")
file(GLOB_RECURSE LOGGING_SOURCES "Logging/*.cs")
file(GLOB_RECURSE SERVER_SOURCES "TradingServer/*.cs")
file(GLOB_RECURSE SERVICE_SOURCES "TradingServices/*.cs")

set(ORDERS_DLL_DIRECTORY "Orderbook/Orders/bin/Debug/net9.0/TradingServer.Orders.dll")
set(ORDERBOOK_DLL_DIRECTORY "Orderbook/OrderbookCS/bin/Debug/net9.0/TradingServer.Orderbook.dll")
set(LOGGING_DLL_DIRECTORY "Logging/bin/Debug/net9.0/TradingServer.Logging.dll")
set(SERVICES_DLL_DIRECTORY "TradingServices/bin/Debug/net9.0/TradingServer.Services.dll")
set(SERVER_DLL_DIRECTORY "TradingServer/bin/Debug/net9.0/TradingServer.Core.dll")

add_custom_command(
        OUTPUT ${ORDERS_DLL_DIRECTORY}
        COMMAND dotnet build Orderbook/Orders/Orders.csproj
        DEPENDS ${ORDERS_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        Orders
        DEPENDS ${ORDERS_DLL_DIRECTORY}
)

add_custom_command(
        OUTPUT ${ORDERBOOK_DLL_DIRECTORY}
        COMMAND dotnet build Orderbook/OrderbookCS/OrderbookCS.csproj
        DEPENDS ${ORDERBOOK_SOURCES} Orders
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        OrderbookCS
        DEPENDS ${ORDERBOOK_DLL_DIRECTORY}
)

add_custom_command(
        OUTPUT ${LOGGING_DLL_DIRECTORY}
        COMMAND dotnet build Logging/Logging.csproj
        DEPENDS ${LOGGING_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        Logging
        DEPENDS ${LOGGING_DLL_DIRECTORY}
)

add_custom_command(
        OUTPUT ${SERVICES_DLL_DIRECTORY}
        COMMAND dotnet build TradingServices/TradingServices.csproj
        DEPENDS ${SERVICE_SOURCES}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        TradingServices
        DEPENDS ${SERVICES_DLL_DIRECTORY}
)

add_custom_command(
        OUTPUT ${SERVER_DLL_DIRECTORY}
        COMMAND dotnet build TradingServer/TradingServer.csproj
        DEPENDS ${SERVER_SOURCES} OrderbookCS Logging TradingServices
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_custom_target(
        TradingServer
        DEPENDS ${SERVER_DLL_DIRECTORY}
)

add_custom_target(BackgroundTradingHost ALL
        COMMAND dotnet run --project TradingServer/TradingServer.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        COMMENT "Starting trading server..."
)

