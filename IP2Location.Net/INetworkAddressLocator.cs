using System.Diagnostics.Contracts;
using System.Net;

namespace IP2Location.Net
{
    public interface INetworkAddressLocator
    {
        [Pure]
        Location Lookup(IPAddress ip);
    }
}