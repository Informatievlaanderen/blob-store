namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Linq;

    public readonly struct ContentMediaType : IEquatable<ContentMediaType>
    {
        public const int MaxLength = 64;

        private static readonly string[] KnownMediaTypes =
        {
            "application",
            "audio",
            "font",
            "example",
            "image",
            "message",
            "model",
            "multipart",
            "text",
            "video",
            "binary"
        };

        private readonly string _value;

        private ContentMediaType(string value)
        {
            _value = value;
        }

        public static bool CanParse(string value) => TryParse(value, out _);

        public static bool TryParse(string value, out ContentMediaType parsed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length > MaxLength)
            {
                parsed = default;
                return false;
            }

            if (value.StartsWith("X-"))
            {
                if (value.Length == 2 ||
                    value.Skip(2).Any(tokenCharacter => !TokenCharacter.IsAcceptable(tokenCharacter)))
                {
                    parsed = default;
                    return false;
                }
            } else if (!Array.Exists(
                    KnownMediaTypes,
                    candidate => string.Equals(candidate, value, StringComparison.InvariantCulture)))
            {
                parsed = default;
                return false;
            }

            parsed = new ContentMediaType(value);
            return true;
        }

        public static ContentMediaType Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value == string.Empty)
            {
                throw new FormatException("The content media type value can not be empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new FormatException($"The content media type value can not exceed {MaxLength} characters.");
            }

            if (value.StartsWith("X-"))
            {
                if (value.Length == 2)
                {
                    throw new FormatException(
                        "The custom content media type value must be longer than 2 characters.");
                }
                if (value.Skip(2).Any(tokenCharacter => !TokenCharacter.IsAcceptable(tokenCharacter)))
                {
                    throw new FormatException(
                        $"The custom content media type value must not contain spaces, control characters nor one of the unacceptable token characters {string.Join(", ", TokenCharacter.UnacceptableCharacters.Select(candidate => "'" + candidate + "'"))}.");
                }
            }
            else if (!Array.Exists(KnownMediaTypes, candidate => string.Equals(candidate, value, StringComparison.InvariantCulture)))
            {
                throw new FormatException(
                    $"The content media type value must be either a private media type starting with 'X-' or one of {string.Join(", ", KnownMediaTypes.Select(candidate => "'" + candidate + "'"))}.");
            }

            return new ContentMediaType(value);
        }

        public bool Equals(ContentMediaType other) => _value == other._value;
        public override bool Equals(object? other) => other is ContentMediaType instance && Equals(instance);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(ContentMediaType instance) => instance._value;
        public static bool operator ==(ContentMediaType left, ContentMediaType right) => left.Equals(right);
        public static bool operator !=(ContentMediaType left, ContentMediaType right) => !left.Equals(right);
    }
}
