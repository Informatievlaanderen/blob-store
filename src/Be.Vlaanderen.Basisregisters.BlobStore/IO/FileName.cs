namespace Be.Vlaanderen.Basisregisters.BlobStore.IO
{
    using System;
    using System.Text;

    internal static class FileName
    {
        public static string From(BlobName name)
        {
            var value = name.ToString().ToCharArray();
            var builder = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; index++)
            {
                switch (value[index])
                {
                    case '!':
                        builder.Append(Convert.ToByte('!').ToString("x"));
                        break;
                    case '/':
                        builder.Append(Convert.ToByte('/').ToString("x"));
                        break;
                    case '-':
                        builder.Append(Convert.ToByte('-').ToString("x"));
                        break;
                    case '_':
                        builder.Append(Convert.ToByte('_').ToString("x"));
                        break;
                    case '.':
                        builder.Append(Convert.ToByte('.').ToString("x"));
                        break;
                    case '*':
                        builder.Append(Convert.ToByte('*').ToString("x"));
                        break;
                    case '\'':
                        builder.Append(Convert.ToByte('\'').ToString("x"));
                        break;
                    case '(':
                        builder.Append(Convert.ToByte('(').ToString("x"));
                        break;
                    case ')':
                        builder.Append(Convert.ToByte(')').ToString("x"));
                        break;
                    default:
                        builder.Append(value[index]);
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
