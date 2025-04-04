namespace Be.Vlaanderen.Basisregisters.BlobStore.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileBlobClient : IBlobClient
    {
        private readonly DirectoryInfo _directory;

        public FileBlobClient(DirectoryInfo directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public Task<BlobObject?> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<BlobObject?>(cancellationToken);
            }

            var file = new FileInfo(Path.Combine(_directory.FullName, FileName.From(name)));
            if (!file.Exists)
            {
                return Task.FromResult<BlobObject?>(null);
            }

            using (var fileStream = file.OpenRead())
            using (var reader = new BinaryReader(fileStream, Encoding.UTF8))
            {
                var metadata = Metadata.None;
                var contentType = ContentType.Parse(reader.ReadString());
                var metadatumCount = reader.ReadInt32();
                for (var index = 0; index < metadatumCount; index++)
                {
                    var key = new MetadataKey(reader.ReadString());
                    var valueLength = reader.ReadInt32();
                    var value = valueLength != -1 ? reader.ReadString() : null;
                    metadata = metadata.Add(new KeyValuePair<MetadataKey, string>(key, value));
                }

                return Task.FromResult(new BlobObject(name, metadata, contentType, contentCancellationToken =>
                {
                    if (!File.Exists(file.FullName))
                    {
                        throw new BlobNotFoundException(name);
                    }

                    var contentFileStream = file.OpenRead();
                    using (var contentReader = new BinaryReader(contentFileStream, Encoding.UTF8, true))
                    {
                        // skip over the metadata
                        contentReader.ReadString();
                        var contentMetadatumCount = contentReader.ReadInt32();
                        for (var index = 0; index < contentMetadatumCount; index++)
                        {
                            contentReader.ReadString();
                            var valueLength = contentReader.ReadInt32();
                            if(valueLength != -1) contentReader.ReadString();
                        }
                    }
                    return Task.FromResult<Stream>(new ForwardOnlyStream(contentFileStream));
                }));
            }
        }

        public Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<bool>(cancellationToken);
            }
            var file = new FileInfo(Path.Combine(_directory.FullName, FileName.From(name)));
            return Task.FromResult(file.Exists);
        }

        public async Task CreateBlobAsync(
            BlobName name,
            Metadata metadata,
            ContentType contentType,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            var file = new FileInfo(Path.Combine(_directory.FullName, FileName.From(name)));
            if (file.Exists)
            {
                throw new BlobAlreadyExistsException(name);
            }

            using (var fileStream = file.OpenWrite())
            {
                using (var writer = new BinaryWriter(fileStream, Encoding.UTF8, true))
                {
                    writer.Write(contentType.ToString()); // content type
                    writer.Write(metadata.Count); // count of metadatum
                    foreach (var metadatum in metadata)
                    {
                        writer.Write(metadatum.Key.ToString()); // key
                        writer.Write(metadatum.Value?.Length ?? -1); // length of value - null is indicated using -1
                        if (metadatum.Value != null)
                        {
                            writer.Write(metadatum.Value); // non null value
                        }
                    }
                }
                content.CopyTo(fileStream);
                await fileStream.FlushAsync(cancellationToken);
            }
        }

        public Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            var file = new FileInfo(Path.Combine(_directory.FullName, FileName.From(name)));
            if (file.Exists) { file.Delete(); }
            return Task.CompletedTask;
        }
    }
}
