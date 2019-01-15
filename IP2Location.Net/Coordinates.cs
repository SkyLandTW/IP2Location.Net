using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace IP2Location.Net
{
    public struct Coordinates : IEquatable<Coordinates>
    {
        [Required] public readonly float Latitude;
        [Required] public readonly float Longitude;

        public Coordinates(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        [Pure]
        public bool Equals(Coordinates other)
        {
            // one latitude = 111km max; consider two values equal when below 0.9mm
            return Math.Abs(Latitude - other.Latitude) < 1e-8f
                   && Math.Abs(Longitude - other.Longitude) < 1e-8f;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Coordinates other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Latitude:F4}, {Longitude:F4}";
        }
    }
}