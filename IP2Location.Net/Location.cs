using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace IP2Location.Net
{
    public struct Location : IEquatable<Location>
    {
        [Required] public readonly string CountryCode;
        [Required] public readonly string Country;
        [Required] public readonly string Region;
        [Required] public readonly string City;
        [Required] public readonly Coordinates Coordinates;

        public Location(string countryCode, string country, string region, string city, Coordinates coordinates)
        {
            CountryCode = countryCode;
            Country = country;
            Region = region;
            City = city;
            Coordinates = coordinates;
        }

        [Pure]
        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CountryCode, other.CountryCode)
                   && string.Equals(Country, other.Country)
                   && string.Equals(Region, other.Region)
                   && string.Equals(City, other.City)
                   && Coordinates.Equals(other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Location other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CountryCode.GetHashCode();
                hashCode = (hashCode * 397) ^ Country.GetHashCode();
                hashCode = (hashCode * 397) ^ Region.GetHashCode();
                hashCode = (hashCode * 397) ^ City.GetHashCode();
                hashCode = (hashCode * 397) ^ Coordinates.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"[{CountryCode}] {Country}\\{Region}\\{City}, {Coordinates}";
        }
    }
}