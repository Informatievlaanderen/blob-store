namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;

    public static class Customizations
    {
        public static IPostprocessComposer<T> FromFactory<T>(this IFactoryComposer<T> composer, Func<Random, T> factory)
        {
            return composer.FromFactory<int>(value => factory(new Random(value)));
        }

        public static void CustomizeBlobName(this IFixture fixture)
        {
            fixture.Customize<BlobName>(customization =>
                customization.FromFactory<int>(value =>
                    new BlobName(new string((char) new Random(value).Next(97, 123), new Random(value).Next(1, BlobName.MaxLength)))
                ));
        }

        public static void CustomizeBlobName(this IFixture fixture, int maxLength)
        {
            if (maxLength <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            fixture.Customize<BlobName>(customization =>
                customization.FromFactory<int>(value =>
                    new BlobName(new string((char) new Random(value).Next(97, 123), new Random(value).Next(1, Math.Min(maxLength, BlobName.MaxLength))))
                ));
        }

        public static void CustomizeContentMediaSubtype(this IFixture fixture)
        {
            fixture.Customize<ContentMediaSubtype>(customization =>
                customization.FromFactory<int>(value =>
                    ContentMediaSubtype.Parse(CreateContentMediaSubtypeString(value))
                ));
        }

        public static void CustomizeContentMediaSubtypeString(this IFixture fixture)
        {
            fixture.Customize<string>(customization =>
                customization.FromFactory<int>(CreateContentMediaSubtypeString));
        }

        private static string CreateContentMediaSubtypeString(int seed)
        {
            return new string('a', new Random(seed).Next(1, ContentMediaSubtype.MaxLength + 1));
        }

        public static void CustomizeContentMediaType(this IFixture fixture)
        {
            fixture.Customize<ContentMediaType>(customization =>
                customization.FromFactory<int>(value => ContentMediaType.Parse(CreateContentMediaTypeString(value))));
        }

        private static string CreateContentMediaTypeString(int seed)
        {
            var result = "";
            switch (seed % 11)
            {
                case 0:
                    result = "X-" + new string('a', new Random(seed).Next(1, ContentMediaType.MaxLength - 2 + 1));
                    break;
                case 1:
                    result = "application";
                    break;
                case 2:
                    result = "audio";
                    break;
                case 3:
                    result = "font";
                    break;
                case 4:
                    result = "example";
                    break;
                case 5:
                    result = "image";
                    break;
                case 6:
                    result = "message";
                    break;
                case 7:
                    result = "model";
                    break;
                case 8:
                    result = "multipart";
                    break;
                case 9:
                    result = "text";
                    break;
                case 10:
                    result = "video";
                    break;
            }

            return result;
        }

        public static void CustomizeContentMediaTypeString(this IFixture fixture)
        {
            fixture.Customize<string>(customization =>
                customization.FromFactory<int>(CreateContentMediaTypeString));
        }

        public static void CustomizeContentType(this IFixture fixture)
        {
            fixture.Customize<ContentType>(customization =>
                customization.FromFactory<int>(value => ContentType.Parse(CreateContentTypeString(value))));
        }

        private static string CreateContentTypeString(int seed)
        {
            return CreateContentMediaTypeString(seed) + "/" + CreateContentMediaSubtypeString(seed);
        }

        public static void CustomizeContentTypeString(this IFixture fixture)
        {
            fixture.Customize<string>(customization =>
                customization.FromFactory<int>(CreateContentTypeString));
        }

        public static void CustomizeMetadataKey(this IFixture fixture)
        {
            fixture.Customize<MetadataKey>(customization =>
                customization.FromFactory<int>(value =>
                    new MetadataKey(CreateMetadataKeyString(value))
                ));
        }

        public static void CustomizeMetadataKeyString(this IFixture fixture)
        {
            fixture.Customize<string>(customization =>
                customization.FromFactory<int>(CreateMetadataKeyString));
        }

        private static string CreateMetadataKeyString(int seed)
        {
            return new string('a', new Random(seed).Next(1, MetadataKey.MaxLength + 1));
        }

        public static void CustomizeMetadata(this IFixture fixture)
        {
            fixture.Customize<Metadata>(customization =>
                customization.FromFactory<int>(value =>
                    fixture
                        .CreateMany<KeyValuePair<MetadataKey, string>>(new Random(value).Next(1, 5))
                        .Aggregate(Metadata.None,
                            (data, metadatum) => data.Add(metadatum))
                ));
        }
    }
}
