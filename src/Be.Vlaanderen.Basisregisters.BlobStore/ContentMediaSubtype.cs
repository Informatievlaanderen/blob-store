namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Linq;

    public readonly struct ContentMediaSubtype : IEquatable<ContentMediaSubtype>
    {
        public const int MaxLength = 64;

        private readonly string _value;

        private ContentMediaSubtype(string value)
        {
            _value = value;
        }

        public static bool CanParse(string value) => TryParse(value, out _);

        public static bool TryParse(string value, out ContentMediaSubtype parsed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value == string.Empty || value.Length > MaxLength || value.Any(tokenCharacter => !TokenCharacter.IsAcceptable(tokenCharacter)))
            {
                parsed = default;
                return false;
            }

            parsed = new ContentMediaSubtype(value);
            return true;
        }

        public static ContentMediaSubtype Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value == string.Empty)
            {
                throw new FormatException("The content media subtype value can not be empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new FormatException($"The content media subtype value can not exceed {MaxLength} characters.");
            }

            if (value.Any(tokenCharacter => !TokenCharacter.IsAcceptable(tokenCharacter)))
            {
                throw new FormatException(
                    $"The content media subtype value must not contain spaces, control characters nor one of the unacceptable token characters {string.Join(", ", TokenCharacter.UnacceptableCharacters.Select(candidate => "'" + candidate + "'"))}");
            }

            return new ContentMediaSubtype(value);
        }

        public bool Equals(ContentMediaSubtype other) => _value == other._value;
        public override bool Equals(object other) => other is ContentMediaSubtype instance && Equals(instance);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(ContentMediaSubtype instance) => instance._value;
        public static bool operator ==(ContentMediaSubtype left, ContentMediaSubtype right) => left.Equals(right);
        public static bool operator !=(ContentMediaSubtype left, ContentMediaSubtype right) => !left.Equals(right);
    }
}
