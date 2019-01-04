namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using AutoFixture.Kernel;

    public class FrozenSpecimenBuilder<T> : ISpecimenBuilder
    {
        public FrozenSpecimenBuilder(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is SeededRequest seededRequest)
            {
                if (seededRequest.Request is Type type && type == typeof(T))
                {
                    return Value;
                }
            }
            else if (request != null && request.Equals(typeof(T)))
            {
                return Value;
            }

            return new NoSpecimen();
        }
    }
}
