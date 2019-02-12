namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public readonly struct BlobName : IEquatable<BlobName>
    {
        public const int MaxLength = 128;

        private readonly string _value;

        private static readonly char[] AcceptableCharacters =
            Enumerable
                .Range(97, 26)
                .Select(value => (char) value) // lower case a-z
            .Concat(
                Enumerable
                    .Range(65, 26)
                    .Select(value => (char) value)) // upper case A-Z
            .Concat(
                Enumerable
                    .Range(48, 10)
                    .Select(value => (char) value)) // 0-9
            .Concat(new[] { '!', '/', '-', '_', '.', '*', '\'', '(', ')' })
            .ToArray();

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

            if (value.Any(character => !Array.Exists(AcceptableCharacters, candidate => candidate == character)))
            {
                throw new ArgumentException(
                    $"The blob name can only contain acceptable characters ({string.Join(", ", AcceptableCharacters.Select(acceptable => "'" + acceptable + "'"))}).",
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
