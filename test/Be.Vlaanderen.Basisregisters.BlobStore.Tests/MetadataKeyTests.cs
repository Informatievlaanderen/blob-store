namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Framework;
    using Xunit;

    public class MetadataKeyTests
    {
        private readonly Fixture _fixture;

        public MetadataKeyTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeMetadataKey();
        }

        [Fact]
        public void MaximumLengthReturnsExpectedValue()
        {
            Assert.Equal(128, MetadataKey.MaxLength);
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.CustomizeMetadataKeyString();
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(
                    _fixture,
                    new CompositeBehaviorExpectation(
                        new NullReferenceBehaviorExpectation(),
                        new EmptyStringBehaviorExpectation()
                    )
                ),
                new ImplicitConversionOperatorAssertion<string>(
                    new CompositeSpecimenBuilder(customizedString, _fixture)),
                new EquatableEqualsSelfAssertion(_fixture),
                new EquatableEqualsOtherAssertion(
                    new CompositeSpecimenBuilder(customizedString, _fixture)),
                new EqualityOperatorEqualsSelfAssertion(_fixture),
                new EqualityOperatorEqualsOtherAssertion(_fixture),
                new InequalityOperatorEqualsSelfAssertion(_fixture),
                new InequalityOperatorEqualsOtherAssertion(_fixture),
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsOtherAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(MetadataKey));
        }

        [Fact]
        public void ValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MetadataKey(null));
        }

        [Fact]
        public void ValueCanNotBeEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new MetadataKey(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void ValueIsWellformed(string value)
        {
            Assert.NotEqual(default, new MetadataKey(value));
        }

        public static IEnumerable<object[]> WellformedCases
        {
            get { return WellformedMetadataKeys.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> WellformedMetadataKeys
        {
            get
            {
                var random = new Random();

                var acceptableCharacters =
                    Enumerable
                        .Range(97, 26)
                        .Select(value => (char) value) // lower case a-z
                        .Concat(
                            Enumerable
                                .Range(65, 26)
                                .Select(value => (char) value)) // upper case A-Z
                        .Concat(
                            Enumerable
                                .Range(48, 10)
                                .Select(value => (char) value)) // 0-9
                        .Concat(new[] {'.', '-', '_'}) // . - _
                        .ToArray();

                var fixture = new Fixture();
                fixture.Customizations.Add(
                    new FiniteSequenceGenerator<char>(acceptableCharacters));

                yield return new string(fixture.CreateMany<char>(random.Next(1, MetadataKey.MaxLength + 1)).ToArray());
            }
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void ValueIsNotWellformed(string value)
        {
            Assert.Throws<ArgumentException>(() => new MetadataKey(value));
        }

        public static IEnumerable<object[]> NotWellformedCases
        {
            get { return NotWellformedMetadataKeys.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> NotWellformedMetadataKeys
        {
            get
            {
                var generator = new Generator<char>(new Fixture());

                var acceptableCharacters =
                    Enumerable
                        .Range(97, 26)
                        .Select(value => (char) value) // lower case a-z
                        .Concat(
                            Enumerable
                                .Range(65, 26)
                                .Select(value => (char) value)) // upper case A-Z
                        .Concat(
                            Enumerable
                                .Range(48, 10)
                                .Select(value => (char) value)) // 0-9
                        .Concat(new[] {'.', '-', '_'}) // . - _
                        .ToArray();

                yield return new string(
                    generator
                        .Where(candidate => !Array.Exists(acceptableCharacters, character => character == candidate))
                        .Take(3)
                        .ToArray()
                );
            }
        }
    }
}
