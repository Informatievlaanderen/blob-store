namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;

    public readonly struct ContentType : IEquatable<ContentType>
    {
        public const int MaxLength = ContentMediaType.MaxLength + 1 + ContentMediaSubtype.MaxLength;

        private readonly ContentMediaType _type;
        private readonly ContentMediaSubtype _subtype;

        private ContentType(ContentMediaType type, ContentMediaSubtype subtype)
        {
            _type = type;
            _subtype = subtype;
        }

        public static bool CanParse(string value) => TryParse(value, out _);

        public static bool TryParse(string value, out ContentType parsed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var parts = value.Split('/');
            if (parts.Length == 2 &&
                ContentMediaType.TryParse(parts[0], out var type) &&
                ContentMediaSubtype.TryParse(parts[1], out var subtype))
            {
                parsed = new ContentType(type, subtype);
                return true;
            }

            parsed = default;
            return false;
        }

        public static ContentType Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var parts = value.Split('/');
            if (parts.Length != 2)
            {
                throw new FormatException("The content type value is not a well formed 'type/subtype'.");
            }

            var type = ContentMediaType.Parse(parts[0]);
            var subtype = ContentMediaSubtype.Parse(parts[1]);

            return new ContentType(type, subtype);
        }

        public bool Equals(ContentType other) => _type == other._type && _subtype == other._subtype;
        public override bool Equals(object? other) => other is ContentType instance && Equals(instance);
        public override int GetHashCode() => _type.GetHashCode() ^ _subtype.GetHashCode();
        public override string ToString() => _type + "/" + _subtype;
        public static implicit operator string(ContentType instance) => instance.ToString();
        public static bool operator ==(ContentType left, ContentType right) => left.Equals(right);
        public static bool operator !=(ContentType left, ContentType right) => !left.Equals(right);
    }
}
