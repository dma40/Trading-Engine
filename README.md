# Trading-Engine

A simplified simulation of modern day exchange trading, which 

- supports multiple order types available on stock exchanges
- supports text logging, trace logging, database logging, and console logging
- uses gRPC to send order information to the trading server

(Note: in order to use DatabaseLogger you need to have a MySQL server running on your computer, which will most likely be running at port 3306. Also, the trace logger is still being built, it may not function optimally!)
