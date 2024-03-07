# Be.Vlaanderen.Basisregisters.BlobStore [![Build Status](https://github.com/Informatievlaanderen/blob-store/workflows/Build/badge.svg)](https://github.com/Informatievlaanderen/blob-store/actions)

Blob storage abstraction on top of AWS S3, Memory, Files and SQL Server.
It's a bit leaky in that not all stores behave exactly in the same way and have different constraints towards names and 
metadata. Choose your blob names and metadata keys/values carefully - keep it simple.
AWS S3 has no real concurrency mechanism and neither does Files.

## Usage

```csharp
// Memory
var memoryClient = new MemoryBlobClient();

// Files
var fileClient = new FileBlobClient(new DirectoryInfo("path-where-the-blobs-will-be-stored"));

// AWS S3
AmazonS3Client CreateClient()
{
  return new AmazonS3Client(
    new BasicAWSCredentials("your-access-key","your-secret"),
    CreateClientConfig());
}

AmazonS3Config CreateClientConfig()
{
  return new AmazonS3Config
  {
    LogMetrics = false,
    DisableLogging = true,
    DisableHostPrefixInjection = true,
    ThrottleRetries = false,
    UseHttp = true,
    UseDualstackEndpoint = false,
    UseAccelerateEndpoint = false,
    EndpointDiscoveryEnabled = false,
    ForcePathStyle = true,
    ServiceURL = $"http://localhost:{_hostPort}"
  };
}
var s3Client = new S3BlobClient(CreateClient(), "bucket-where-the-blobs-will-be-stored");

// SQL Server
var connectionStringBuilder = new SqlConnectionStringBuilder("connection-string-of-database-where-the-blobs-will-be-stored");
var sqlClient = new SqlBlobClient(connectionStringBuilder, "schema-within-said-database");
```

The behavior you get across all of those is exposed via `IBlobClient`

```csharp
public interface IBlobClient
{
  // get a blob from storage including its name, metadata, content type and a way to open the content stream
  Task<BlobObject> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default);
  
  // check if the blob is already in storage
  Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default);

  // create a new blob in storage with name, metadata, content type and a content stream
  Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content, CancellationToken cancellationToken = default);

  // delete a blob from storage
  Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default);
}
```
