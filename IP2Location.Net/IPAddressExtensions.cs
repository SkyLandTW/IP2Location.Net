using System;
using System.Diagnostics.Contracts;
using System.Net;

namespace IP2Location.Net
{
    public static class IPAddressExtensions
    {
        [Pure]
        public static uint GetIPv4Value(this IPAddress ipAddress)
        {
            var addressBytes = ipAddress.MapToIPv4().GetAddressBytes();
            if (addressBytes.Length != 4)
                throw new InvalidOperationException();
            var ipValue = ((uint) addressBytes[0] << 24)
                          | ((uint) addressBytes[1] << 16)
                          | ((uint) addressBytes[2] << 8)
                          | addressBytes[3];
            return ipValue;
        }
    }
}