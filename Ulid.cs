using System;

namespace UlidFramework
{
    public struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
    {
        private const int TimestampLength = 6;
        private const int RandomLength = 10;
        internal const int UlidLength = TimestampLength + RandomLength;
        
        public byte[] Bytes { get; }

        public Ulid(byte[] bytes)
        {
            if (bytes.Length != UlidLength) throw new ArgumentException($"ULID must be {UlidLength} bytes.");
            Bytes = bytes;
        }

        public static Ulid NewUlid() => NewUlidRng();
        public static Ulid NewUlidRng()
        {
            UlidInteropStruct str = new UlidInteropStruct
            {
                data = new byte[UlidLength]
            };
            
            UlidHelper.NewUlid(ref str, UlidRng.GetRandomInteger(0));

            return str;
        }

        public override string ToString()  => UlidHelper.ToStringSafe(ref this);

        public Guid ToGuid()
        {
            var guidBytes = new byte[16];
            Array.Copy(Bytes, guidBytes, Math.Min(Bytes.Length, guidBytes.Length));
            return new Guid(guidBytes);
        }

        public DateTime ExtractDateTimeUtc() => (UlidHelper.ExtractDateTimeUtc(ref this));
        public DateTime ExtractDateTime() => (UlidHelper.ExtractDateTime(ref this));
        
        public static bool TryParse(string str, out Ulid ulid)
        {
            return UlidHelper.TryParse(str, out ulid);
        }

        public static bool operator ==(Ulid o1, Ulid o2) => o1.CompareTo(o2) == 0;
        public static bool operator !=(Ulid o1, Ulid o2) => o1.CompareTo(o2) != 0;
        public int CompareTo(Ulid other)
        {
            var @this = (UlidInteropStruct)this;
            var _other = (UlidInteropStruct)other;
            return UlidHelper.CompareUlid(ref @this, ref _other);
        }

        public bool Equals(Ulid other)
        {
            return Equals(Bytes, other.Bytes);
        }

        public override bool Equals(object obj)
        {
            return obj is Ulid other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Bytes != null ? Bytes.GetHashCode() : 0);
        }

        public int CompareTo(object other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            if (other is Ulid _other)
                return CompareTo(_other);

            throw new ArgumentException("Value must be of type Ulid", nameof(other));
        }
    }
}
