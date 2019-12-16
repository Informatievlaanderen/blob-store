namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class EquatableEqualsOtherAssertion : IdiomaticAssertion
    {
        public EquatableEqualsOtherAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var equatableType = typeof(IEquatable<>).MakeGenericType(type);
            if (!equatableType.IsAssignableFrom(type))
                throw new EquatableEqualsException(type, $"The type {type.Name} does not implement IEquatable<{type.Name}>.");

            var method = equatableType.GetMethods().Single();

            var constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructorInfos.Length == 1 && constructorInfos[0].GetParameters().Length == 1)
            {
                var constructorInfo = constructorInfos[0];
                var parameterInfo = constructorInfo.GetParameters()[0];
                var left = Builder.CreateAnonymous(parameterInfo.ParameterType);
                var right = Builder.CreateAnonymous(parameterInfo.ParameterType);
                while (left.Equals(right))
                {
                    right = Builder.CreateAnonymous(parameterInfo.ParameterType);
                }
                var self = constructorInfo.Invoke(new [] { left });
                var other = constructorInfo.Invoke(new [] { right });

                object result;
                try
                {
                    result = method.Invoke(self, new[] { other });
                }
                catch (Exception exception)
                {
                    throw new EquatableEqualsException(type, $"The IEquatable<{type.Name}>.Equals method of type {type.Name} threw an exception: {exception}", exception);
                }
                if ((bool)result) throw new EquatableEqualsException(type);
            }
            else
            {
                var self = Builder.CreateAnonymous(type);
                var other = Builder.CreateAnonymous(type);

                object result;
                try
                {
                    result = method.Invoke(self, new[] {other});
                }
                catch (Exception exception)
                {
                    throw new EquatableEqualsException(type,
                        $"The IEquatable<{type.Name}>.Equals method of type {type.Name} threw an exception: {exception}",
                        exception);
                }

                if ((bool) result) throw new EquatableEqualsException(type);
            }
        }
    }
}
