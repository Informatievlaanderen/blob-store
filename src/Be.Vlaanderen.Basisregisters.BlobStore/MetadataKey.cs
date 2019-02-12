namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Linq;

    public readonly struct MetadataKey : IEquatable<MetadataKey>, IComparable<MetadataKey>
    {
        public const int MaxLength = 128;

        private static readonly char[] AcceptableCharacters =
            Enumerable
                .Range(97, 26)
                .Select(value => (char)value) // lower case a-z
            .Concat(
                Enumerable
                    .Range(65, 26)
                    .Select(value => (char)value)) // upper case A-Z
            .Concat(
                Enumerable
                    .Range(48, 10)
                    .Select(value => (char)value)) // 0-9
            .Concat(new [] { '.', '-', '_' }) // . - _
            .ToArray();

        private readonly string _value;

        public MetadataKey(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

            if (value.Any(character => !Array.Exists(AcceptableCharacters, candidate => candidate == character)))
            {
                throw new ArgumentException(
                    $"The blob metadata key value can only contain acceptable characters ({string.Join(", ", AcceptableCharacters.Select(acceptable => "'" + acceptable + "'"))}).",
                    nameof(value));
            }

            _value = value.ToLowerInvariant();
        }

        public MetadataKey WithPrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            return _value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? this : new MetadataKey(prefix + _value);
        }

        public MetadataKey WithoutPrefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            return !_value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? this : new MetadataKey(_value.Substring(prefix.Length));
        }

        public int CompareTo(MetadataKey other) => string.CompareOrdinal(_value, other._value);

        public bool Equals(MetadataKey other) => _value == other._value;
        public override bool Equals(object other) => other is MetadataKey instance && Equals(instance);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(MetadataKey instance) => instance._value;
        public static bool operator ==(MetadataKey left, MetadataKey right) => left.Equals(right);
        public static bool operator !=(MetadataKey left, MetadataKey right) => !left.Equals(right);
    }
}
