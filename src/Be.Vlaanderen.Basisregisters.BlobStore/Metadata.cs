namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class Metadata : IReadOnlyCollection<KeyValuePair<MetadataKey, string>>, IEquatable<Metadata>
    {
        public static readonly Metadata None = new Metadata(ImmutableList<KeyValuePair<MetadataKey, string>>.Empty);

        private readonly ImmutableList<KeyValuePair<MetadataKey, string>> _metadata;

        private Metadata(ImmutableList<KeyValuePair<MetadataKey, string>> metadata)
        {
            _metadata = metadata;
        }

        public Metadata Add(KeyValuePair<MetadataKey, string> metadatum)
        {
            return new Metadata(_metadata.Add(metadatum));
        }

        public bool Equals(Metadata? other) => other is not null &&
                                               _metadata
                                                   .OrderBy(left => left.Key)
                                                   .SequenceEqual(
                                                       other._metadata
                                                           .OrderBy(right => right.Key),
                                                       MetadatumComparer.Instance);
        public override bool Equals(object? obj) => obj is Metadata other && Equals(other);
        public override int GetHashCode() => _metadata.Aggregate(0, (hashCode, current) =>
            current.Key.GetHashCode() ^ current.Value?.GetHashCode() ?? 0 ^ hashCode);
        public static bool operator ==(Metadata left, Metadata right) => Equals(left, right);
        public static bool operator !=(Metadata left, Metadata right) => !Equals(left, right);
        public IEnumerator<KeyValuePair<MetadataKey, string>> GetEnumerator() => _metadata.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _metadata.Count;

        private class MetadatumComparer : IEqualityComparer<KeyValuePair<MetadataKey, string>>
        {
            public static readonly IEqualityComparer<KeyValuePair<MetadataKey, string>> Instance = new MetadatumComparer();

            public bool Equals(KeyValuePair<MetadataKey, string> left, KeyValuePair<MetadataKey, string> right)
            {
                return left.Key.Equals(right.Key)
                       && Equals(left.Value, right.Value);
            }

            public int GetHashCode(KeyValuePair<MetadataKey, string> obj)
            {
                throw new NotSupportedException();
            }
        }
    }
}
