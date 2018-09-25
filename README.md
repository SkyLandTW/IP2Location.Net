IP2Location API for .NET Core
=============================

Adapted from the official [C library](https://github.com/chrislim2888/IP2Location-C-Library) for .NET Core

There are two versions:

1. For Memory mapped binary file such as *IP2LOCATION-LITE-DB5.BIN*.
2. For CSV file (very slow at startup), just for references.

only DB5 is supported


Please refer to the official [C library](https://github.com/chrislim2888/IP2Location-C-Library) regarding license.


Usage
-----

    INetworkAddressLocator << NetworkAddressLocator_IP2LocationBin
                           << NetworkAddressLocator_IP2LocationCsv

The INetworkAddressLocator has a single method *Lookup(IPAddress)* which returns a Location.
Missing and invalid locations are represented by "-" in every fields in IP2Location database, instead of *NULL*.