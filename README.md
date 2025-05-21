# Trading-Engine

A simplified simulation of modern day exchange trading, which 

- supports multiple order types available on stock exchanges
- supports text logging, trace logging, database logging, and console logging
- uses gRPC to send order information to the trading server

To run this project successfully, you need

- .NET 9.0 installed on your computer
- A MySQL connection running on your computer + a viable connection (test your connection and authentication informationfirst before running!)

To run, you first will need to set the environment variables MYSQL_USER and MYSQL_PASS using db_credentials.sh. Run these commands from your terminal in the root directory:

```
chmod +x ./db_credentials.sh
./db_credentials.sh
```

This will set them to the values you declare them to be db_credentials.sh. Make sure that they correspond to valid MySQL user information on your machine!

Then, run 
```
chdir TradingServer
```

to change to TradingServer - the Program.cs file is in that directory. Then, run

```
dotnet build
dotnet run
```

to start the trading server. 