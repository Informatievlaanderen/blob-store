namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;

    public class BlobNotFoundException : BlobClientException
    {
        public BlobName Name { get; }

        public BlobNotFoundException(BlobName name)
            : base($"The blob with name {name} was not found.")
        {
            Name = name;
        }

        public BlobNotFoundException(BlobName name, Exception exception)
            : base($"The blob with name {name} was not found.", exception)
        {
            Name = name;
        }
    }
}
