namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Runtime.Serialization;

    public class BlobClientException : Exception
    {
        public BlobClientException()
        {
        }

        public BlobClientException(string message) : base(message)
        {
        }

        public BlobClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BlobClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}