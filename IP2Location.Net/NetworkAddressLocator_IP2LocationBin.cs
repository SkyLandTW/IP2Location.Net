using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IP2Location.Net
{
    public sealed class NetworkAddressLocator_IP2LocationBin : INetworkAddressLocator, IDisposable
    {
        // see https://github.com/chrislim2888/IP2Location-C-Library/blob/master/libIP2Location/IP2Location.c

        private readonly MemoryMappedFile m_memoryMappedFile;
        private readonly MemoryMappedViewAccessor m_memoryMappedView;
        private readonly byte m_databaseType;
        private readonly byte m_databaseColumn;
        private readonly uint m_ipv4DataAddr;
        private readonly uint m_ipv4DataCount;
        private readonly uint m_ipv4IndexAddr;

        public unsafe NetworkAddressLocator_IP2LocationBin(string filePath)
        {
            m_memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
            m_memoryMappedView = m_memoryMappedFile.CreateViewAccessor();
            byte* filePtr = null;
            m_memoryMappedView.SafeMemoryMappedViewHandle.AcquirePointer(ref filePtr);
            try
            {
                var header = (FileHeader*) filePtr;
                m_databaseType = header->DatabaseType;
                m_databaseColumn = header->DatabaseColumn;
                m_ipv4DataAddr = header->Ipv4DataBaseAddr;
                m_ipv4DataCount = header->Ipv4DataBaseCount;
                m_ipv4IndexAddr = header->IPv4IndexBaseAddr;
            }
            finally
            {
                m_memoryMappedView.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        private const uint MAX_IPV4_RANGE = 4294967295U;

        public unsafe Location Lookup(IPAddress ip)
        {
            // https://github.com/chrislim2888/IP2Location-C-Library/blob/master/libIP2Location/IP2Location.c
            // IP2Location_get_ipv4_record()
            var baseaddr = m_ipv4DataAddr;
            var dbcolumn = m_databaseColumn;
            var ipv4indexbaseaddr = m_ipv4IndexAddr;
            var low = 0U;
            var high = m_ipv4DataCount;
            var ipno = ip.MapToIPv4().GetIPv4Value();
            if (ipno == MAX_IPV4_RANGE)
            {
                ipno = ipno - 1;
            }
            byte* filePtr = null;
            m_memoryMappedView.SafeMemoryMappedViewHandle.AcquirePointer(ref filePtr);
            try
            {
                if (ipv4indexbaseaddr > 0)
                {
                    // use the index table 
                    var ipnum1n2 = ipno >> 16;
                    var indexpos = ipv4indexbaseaddr + (ipnum1n2 << 3);
                    low = Read32(filePtr, indexpos);
                    high = Read32(filePtr, indexpos + 4);
                }
                while (low <= high)
                {
                    var mid = unchecked((low + high) >> 1);
                    var ipfrom = Read32(filePtr, baseaddr + mid * dbcolumn * 4);
                    var ipto = Read32(filePtr, baseaddr + (mid + 1) * dbcolumn * 4);
                    if (ipno >= ipfrom && ipno < ipto)
                    {
                        return ReadRecord(filePtr, baseaddr + mid * dbcolumn * 4);
                    }
                    else
                    {
                        if (ipno < ipfrom)
                        {
                            high = mid - 1;
                        }
                        else
                        {
                            low = mid + 1;
                        }
                    }
                }
            }
            finally
            {
                m_memoryMappedView.SafeMemoryMappedViewHandle.ReleasePointer();
            }
            throw new ArgumentOutOfRangeException();
        }

        private unsafe Location ReadRecord(byte* filePtr, uint rowAddr)
        {
            var countryCode = ReadString(filePtr, Read32(filePtr, rowAddr + 4 * (COUNTRY_POSITION[m_databaseType] - 1U)));
            var country = ReadString(filePtr, Read32(filePtr, rowAddr + 4 * (COUNTRY_POSITION[m_databaseType] - 1U)) + 3);
            var region = ReadString(filePtr, Read32(filePtr, rowAddr + 4 * (REGION_POSITION[m_databaseType] - 1U)));
            var city = ReadString(filePtr, Read32(filePtr, rowAddr + 4 * (CITY_POSITION[m_databaseType] - 1U)));
            var latitude = ReadFloat(filePtr, rowAddr + 4 * (LATITUDE_POSITION[m_databaseType] - 1));
            var longitude = ReadFloat(filePtr, rowAddr + 4 * (LONGITUDE_POSITION[m_databaseType] - 1));
            return new Location(countryCode, country, region, city, new Coordinates(latitude, longitude));
        }

        public void Dispose()
        {
            m_memoryMappedView.Dispose();
            m_memoryMappedFile.Dispose();
        }

        // see https://github.com/chrislim2888/IP2Location-C-Library/blob/master/libIP2Location/IP2Loc_DBInterface.c
        // some positions are 1 based, NOT 0 based

        private static unsafe string ReadString(byte* baseAddr, uint position0)
        {
            var size = baseAddr[position0];
            return Encoding.ASCII.GetString(baseAddr + position0 + 1, size);
        }

        private static unsafe float ReadFloat(byte* baseAddr, long position1)
        {
            var ptr = baseAddr + position1 - 1;
            var result = *((float*) ptr);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint Read32(byte* baseAddr, uint position1)
        {
            var ptr = baseAddr + position1 - 1;
            var result = *((uint*) ptr);
            return result;
        }

        private static readonly byte[] COUNTRY_POSITION = { 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        private static readonly byte[] REGION_POSITION = { 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
        private static readonly byte[] CITY_POSITION = { 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        private static readonly byte[] LATITUDE_POSITION = { 0, 0, 0, 0, 0, 5, 5, 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
        private static readonly byte[] LONGITUDE_POSITION = { 0, 0, 0, 0, 0, 6, 6, 0, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FileHeader
        {
            public readonly byte DatabaseType;
            public readonly byte DatabaseColumn;
            public readonly byte DatabaseYear;
            public readonly byte DatabaseMonth;
            public readonly byte DatabaseDay;
            public readonly uint Ipv4DataBaseCount;
            public readonly uint Ipv4DataBaseAddr;
            public readonly uint IPv6DataBaseCount;
            public readonly uint IPv6DataBaseAddr;
            public readonly uint IPv4IndexBaseAddr;
            public readonly uint IPv6IndexBaseAddr;
        }
    }
}