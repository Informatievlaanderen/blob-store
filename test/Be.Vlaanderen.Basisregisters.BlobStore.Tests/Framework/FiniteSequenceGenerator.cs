namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using AutoFixture.Kernel;

    public class FiniteSequenceGenerator<T> : ISpecimenBuilder
    {
        private readonly T[] _sequence;
        private int _cursor;

        public FiniteSequenceGenerator(T[] sequence)
        {
            if(sequence.Length == 0)
                throw new ArgumentException("The sequence can not be empty.", nameof(sequence));
            _sequence = sequence;
            _cursor = 0;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is SeededRequest seededRequest)
            {
                return Create(seededRequest);
            }

            if (request is Type type && type == typeof(T))
            {
                var current = _cursor;
                _cursor = _cursor + 1 != _sequence.Length
                    ? _cursor + 1
                    : 0;
                return _sequence[current];
            }

            return new NoSpecimen();
        }

        private object Create(SeededRequest seededRequest)
        {
            if (seededRequest.Request is Type type && type == typeof(T))
            {
                var current = _cursor;
                _cursor = _cursor + 1 != _sequence.Length
                    ? _cursor + 1
                    : 0;
                return _sequence[current];
            }
            return new NoSpecimen();
        }
    }
}
