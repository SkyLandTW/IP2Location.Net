using System;
using System.IO;
using System.Net;

namespace IP2Location.Net
{
    public sealed class NetworkAddressLocator_IP2LocationCsv : INetworkAddressLocator
    {
        private readonly RecordIP4[] m_records;

        public NetworkAddressLocator_IP2LocationCsv(string filePath)
        {
            var fileLines = File.ReadAllLines(filePath);
            var trailingEof = string.IsNullOrWhiteSpace(fileLines[fileLines.Length - 1]);
            m_records = new RecordIP4[fileLines.Length - (trailingEof ? 1 : 0)];
            for (var i = 0; i < m_records.Length; i++)
            {
                var fields = CsvReader.ParseLine(fileLines[i]);
                var ipFirst = uint.Parse(fields[0]);
                var ipLast = uint.Parse(fields[1]);
                var countryCode = string.Intern(fields[2]);
                var country = string.Intern(fields[3]);
                var region = string.Intern(fields[4]);
                var city = string.Intern(fields[5]);
                var lat = float.Parse(fields[6]);
                var lon = float.Parse(fields[7]);
                m_records[i] = new RecordIP4(ipFirst, ipLast,
                    new Location(countryCode, country, region, city, new Coordinates(lat, lon)));
            }
            GC.Collect();
        }

        public Location Lookup(IPAddress ip)
        {
            var ipValue = ip.MapToIPv4().GetIPv4Value();
            var result = m_records.BinarySearch(RecordIP4.GetKey, ipValue);
            var record = m_records[result.Found ? result.Index : result.Index - 1];
            if (ipValue < record.IP4First)
                throw new InvalidOperationException();
            if (ipValue > record.IP4Last)
                throw new InvalidOperationException();
            return record.IPLocation;
        }

        private class RecordIP4
        {
            public static uint GetKey(RecordIP4 record)
            {
                return record.IP4First;
            }

            public readonly uint IP4First;
            public readonly uint IP4Last;
            public readonly Location IPLocation;

            public RecordIP4(uint ip4First, uint ip4Last, Location ipLocation)
            {
                IP4First = ip4First;
                IP4Last = ip4Last;
                IPLocation = ipLocation;
            }
        }
    }
}