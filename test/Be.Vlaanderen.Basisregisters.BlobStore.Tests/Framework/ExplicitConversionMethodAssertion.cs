namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ExplicitConversionMethodAssertion<TResult> : IdiomaticAssertion
    {

        public ExplicitConversionMethodAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            ValueFactory = null;
            SutFactory = null;
        }

        public ExplicitConversionMethodAssertion(
            Func<TResult> valueFactory,
            Func<TResult, object> sutFactory)
        {
            Builder = null;
            ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            SutFactory = sutFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }

        public ISpecimenBuilder Builder { get; }
        public Func<TResult> ValueFactory { get; }
        public Func<TResult, object> SutFactory { get; }

        public override void Verify(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var method = type
                .GetMethods()
                .SingleOrDefault(candidate =>
                    candidate.Name == "To" + typeof(TResult).Name
                    && candidate.GetParameters().Length == 0
                    && candidate.ReturnParameter.ParameterType == typeof(TResult));

            if (method == null)
            {
                throw new ExplicitConversionMethodException(type, typeof(TResult),
                    $"The type '{type.Name}' does not define an explicit conversion method to type '{typeof(TResult).Name}' called 'To{typeof(TResult).Name}()'.");
            }

            var value = Builder.Create<TResult>();
            var builder = new CompositeSpecimenBuilder(
                new FrozenSpecimenBuilder<TResult>(value),
                Builder
            );
            var instance = builder.CreateAnonymous(type);

            try
            {
                var result = (TResult)method.Invoke(instance, new object[0]);
                if (!result.Equals(value))
                    throw new ExplicitConversionMethodException(type, typeof(TResult));
            }
            catch (Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult),
                    $"The explicit conversion method to type '{typeof(TResult).Name}' of type '{type.Name}' threw an exception.", exception);
            }
        }
    }
}
