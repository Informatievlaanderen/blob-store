namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ImplicitConversionOperatorAssertion<TResult> : IdiomaticAssertion
    {
        public ImplicitConversionOperatorAssertion(ISpecimenBuilder builder)
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
                    candidate.Name == "op_Implicit"
                    && candidate.GetParameters().Length == 1
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.ReturnParameter.ParameterType == typeof(TResult));

            if (method == null)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult),
                    $"The type '{type.Name}' does not define an implicit conversion operator to type '{typeof(TResult).Name}'.");
            }

            var value = Builder.Create<TResult>();
            var builder = new CompositeSpecimenBuilder(
                new FrozenSpecimenBuilder<TResult>(value),
                Builder
            );
            var instance = builder.CreateAnonymous(type);

            object result;
            try
            {
                result = method.Invoke(null, new[] { instance });
            }
            catch (Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult),
                    $"The implicit conversion operator to type '{typeof(TResult).Name}' of type '{type.Name}' threw an exception.", exception);
            }

            if (!((TResult)result).Equals(value))
                throw new ImplicitConversionOperatorException(type, typeof(TResult));
        }
    }
}
