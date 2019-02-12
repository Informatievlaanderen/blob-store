namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
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
            try
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
                        try
                        {
                            var contentResponse = await _client.GetObjectAsync(new GetObjectRequest
                            {
                                BucketName = _bucket,
                                Key = name.ToString()
                            }, contentCancellationToken);
                            return contentResponse.ResponseStream;
                        }
                        catch (AmazonS3Exception exception) when (
                            exception.ErrorType == ErrorType.Sender
                            && string.Equals(exception.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new BlobNotFoundException(name, exception);
                        }
                    });
            }
            catch (AmazonS3Exception exception) when (
                exception.ErrorType == ErrorType.Sender
                && string.Equals(exception.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        public async Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = name.ToString(),
                    ByteRange = NoData
                }, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception exception) when (
                exception.ErrorType == ErrorType.Sender
                && string.Equals(exception.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        private static Metadata ConvertMetadataFromMetadataCollection(GetObjectResponse response) =>
            response
                .Metadata
                .Keys
                .Aggregate(
                    Metadata.None,
                    (current, key) =>
                        current.Add(
                            new KeyValuePair<MetadataKey, string>(
                                new MetadataKey(key).WithoutPrefix("x-amz-meta-"),
                                response.Metadata[key])));

        public async Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content,
            CancellationToken cancellationToken = default)
        {
            // S3 does not have real concurrency control, this is simply a best effort approach
            if (await BlobExistsAsync(name, cancellationToken))
            {
                throw new BlobAlreadyExistsException(name);
            }

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
            await _client.PutObjectAsync(request, cancellationToken);
        }

        private static void CopyMetadataToMetadataCollection(Metadata source, MetadataCollection destination)
        {
            foreach (var item in source)
            {
                destination.Add(item.Key.WithPrefix("x-amz-meta-").ToString(), item.Value);
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
