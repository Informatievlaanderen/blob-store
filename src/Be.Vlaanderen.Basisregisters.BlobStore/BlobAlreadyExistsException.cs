namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;

    public class BlobAlreadyExistsException : BlobClientException
    {
        public BlobName Name { get; }

        public BlobAlreadyExistsException(BlobName name)
            : base($"The blob with name {name} already exists.")
        {
            Name = name;
        }

        public BlobAlreadyExistsException(BlobName name, Exception exception)
            : base($"The blob with name {name} already exists.", exception)
        {
            Name = name;
        }
    }
}
