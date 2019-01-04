namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ComparableCompareToSelfAssertion : IdiomaticAssertion
    {
        public ComparableCompareToSelfAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var equatableType = typeof(IComparable<>).MakeGenericType(type);
            if (!equatableType.IsAssignableFrom(type))
            {
                throw new ComparableCompareToException(type, $"The type {type.Name} does not implement IComparable<{type.Name}>.");
            }

            var method = equatableType.GetMethods().Single();
            var self = Builder.CreateAnonymous(type);

            try
            {
                var result = (int)method.Invoke(self, new[] { self });
                if (result != 0)
                    throw new ComparableCompareToException(type);
            }
            catch (Exception exception)
            {
                throw new ComparableCompareToException(type, $"The IComparable<{type.Name}>.Compare method of type {type.Name} threw an exception.", exception);
            }
        }
    }
}
