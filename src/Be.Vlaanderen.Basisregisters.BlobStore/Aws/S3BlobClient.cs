namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;

    public class S3BlobClient : IBlobClient
    {
        private static readonly ByteRange NoData = new ByteRange(0L, 0L);
        private readonly AmazonS3Client _client;
        private readonly string _bucket;

        public S3BlobClient(AmazonS3Client client, string bucket)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        }

        public async Task<BlobObject> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucket,
                Key = name.ToString(),
                ByteRange = NoData
            }, cancellationToken);
            return new BlobObject(
                name,
                ConvertMetadataFromMetadataCollection(response),
                ContentType.Parse(response.Headers.ContentType),
                async contentCancellationToken =>
                {
                    var contentResponse = await _client.GetObjectAsync(new GetObjectRequest
                    {
                        BucketName = _bucket,
                        Key = name.ToString()
                    }, contentCancellationToken);
                    return contentResponse.ResponseStream;
                });
        }

        private static Metadata ConvertMetadataFromMetadataCollection(GetObjectResponse response) =>
            response
                .Metadata
                .Keys
                .Aggregate(
                    Metadata.None,
                    (current, key) =>
                        current.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey(key), response.Metadata[key])));

        public async Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content,
            CancellationToken cancellationToken = default)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = name.ToString(),
                ContentType = contentType.ToString(),
                InputStream = content,
                AutoResetStreamPosition = false,
                AutoCloseStream = false
            };
            CopyMetadataToMetadataCollection(metadata, request.Metadata);
            try
            {
                await _client.PutObjectAsync(request, cancellationToken);
            }
            catch (AmazonS3Exception exception)
            {
                throw new BlobAlreadyExistsException(name, exception);
            }
        }

        private static void CopyMetadataToMetadataCollection(Metadata source, MetadataCollection destination)
        {
            foreach (var item in source)
            {
                destination.Add(item.Key.ToString(), item.Value);
            }
        }

        public async Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            await _client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = name.ToString()
            }, cancellationToken);
        }
    }
}
