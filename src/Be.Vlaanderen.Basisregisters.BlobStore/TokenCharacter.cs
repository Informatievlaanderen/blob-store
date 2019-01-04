namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;

    internal static class TokenCharacter
    {

        public static readonly char[] UnacceptableCharacters =
        {
            '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[', ']', '?', '.', '='
        };

        public static bool IsAcceptable(char value)
        {
            return !char.IsWhiteSpace(value) &&
                   !char.IsControl(value) &&
                   !Array.Exists(UnacceptableCharacters, unacceptable => unacceptable.Equals(value));
        }
    }
}
