namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Diagnostics.Contracts;

    public readonly struct BlobName : IEquatable<BlobName>
    {
        public const int MaxLength = 512;

        private readonly string _value;

        public BlobName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value), "The blob name must not be null or empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentException(
                    $"The blob name must be {MaxLength} characters or less.",
                    nameof(value));
            }

            _value = value;
        }

        public bool Equals(BlobName other) => _value == other._value;
        public override bool Equals(object other) => other is BlobName instance && Equals(instance);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(BlobName instance) => instance._value;
        public static bool operator ==(BlobName left, BlobName right) => left.Equals(right);
        public static bool operator !=(BlobName left, BlobName right) => !left.Equals(right);

        [Pure]
        public BlobName Prepend(BlobName prefix)
        {
            return new BlobName(prefix + _value);
        }

        [Pure]
        public BlobName Append(BlobName suffix)
        {
            return new BlobName(_value + suffix);
        }
    }
}
