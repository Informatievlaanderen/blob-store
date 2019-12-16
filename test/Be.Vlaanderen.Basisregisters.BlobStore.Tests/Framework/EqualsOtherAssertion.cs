namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    /// <summary>
    /// Encapsulates a unit test that verifies that a type which overrides the
    /// <see cref="object.Equals(object)"/> method is implemented correctly with
    /// respect to the rule: calling `x.Equals(y)` returns false.
    /// </summary>
    public class EqualsOtherAssertion : IdiomaticAssertion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsOtherAssertion"/> class.
        /// </summary>
        /// <param name="builder">
        /// A composer which can create instances required to implement the idiomatic unit test,
        /// such as the owner of the property, as well as the value to be assigned and read from
        /// the member.
        /// </param>
        /// <remarks>
        /// <para>
        /// <paramref name="builder" /> will typically be a <see cref="Fixture" /> instance.
        /// </para>
        /// </remarks>
        public EqualsOtherAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Gets the builder supplied by the constructor.
        /// </summary>
        public ISpecimenBuilder Builder { get; }

        /// <summary>
        /// Verifies that `calling `x.Equals(y)' on an instance of the type returns false
        /// if the supplied method is an override of the <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="methodInfo">The method to verify</param>
        public override void Verify(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.ReflectedType == null || !methodInfo.IsObjectEqualsOverrideMethod())
            {
                // The method is not an override of the Object.Equals(object) method
                return;
            }

            var constructorInfos = methodInfo.ReflectedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
                var equalsResult = self.Equals(other);

                if (equalsResult)
                {
                    throw new EqualsOverrideException(string.Format(CultureInfo.CurrentCulture,
                        "The type '{0}' overrides the object.Equals(object) method incorrectly, " +
                        "calling x.Equals(y) should return false.",
                        methodInfo.ReflectedType.FullName));
                }
            }
            else
            {
                var self = Builder.CreateAnonymous(methodInfo.ReflectedType);
                var other = Builder.CreateAnonymous(methodInfo.ReflectedType);
                var equalsResult = self.Equals(other);

                if (equalsResult)
                {
                    throw new EqualsOverrideException(string.Format(CultureInfo.CurrentCulture,
                        "The type '{0}' overrides the object.Equals(object) method incorrectly, " +
                        "calling x.Equals(y) should return false.",
                        methodInfo.ReflectedType.FullName));
                }
            }
        }
    }
}
