namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class GreaterThanOrEqualOperatorCompareToSelfAssertion : IdiomaticAssertion
    {
        public GreaterThanOrEqualOperatorCompareToSelfAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var method = type
                .GetMethods()
                .SingleOrDefault(candidate =>
                    candidate.Name == "op_GreaterThanOrEqual"
                    && candidate.GetParameters().Length == 2
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.GetParameters()[1].ParameterType == type);

            if (method == null)
                throw new EqualityOperatorException(type, $"The type {type.Name} does not implement a greater than or equal operator for {type.Name}.");

            var self = Builder.CreateAnonymous(type);

            object result;
            try
            {
                result = method.Invoke(null, new[] { self, self });
            }
            catch (Exception exception)
            {
                throw new GreaterThanOrEqualOperatorException(type, $"The greater than or equal operator of type {type.Name} threw an exception", exception);
            }
            if (!(bool)result) throw new GreaterThanOrEqualOperatorException(type);
        }
    }
}
