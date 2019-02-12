namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Docker.DotNet.Models;
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
        public void VerifyCtor()
        {
            new GuardClauseAssertion(
                _fixture,
                new CompositeBehaviorExpectation(
                    new NullReferenceBehaviorExpectation(),
                    new EmptyStringBehaviorExpectation()
                )
            ).Verify(Constructors.Select(() => new MetadataKey(null)));
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

        [Theory]
        [MemberData(nameof(WithPrefixCases))]
        public void WithPrefixReturnsExpectedResult(string prefix, MetadataKey sut, MetadataKey expected)
        {
            var result = sut.WithPrefix(prefix);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> WithPrefixCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.CustomizeMetadataKey();
                var key1 = fixture.Create<MetadataKey>();
                yield return new object[]
                {
                    "",
                    key1,
                    key1
                };

                var prefix = fixture.Create<string>();
                var key2 = fixture.Create<MetadataKey>();
                yield return new object[]
                {
                    prefix,
                    key2,
                    new MetadataKey(prefix + key2)
                };

                var upper = prefix.ToUpperInvariant();
                yield return new object[]
                {
                    upper,
                    key2,
                    new MetadataKey(upper + key2)
                };

                yield return new object[]
                {
                    upper,
                    new MetadataKey(prefix + key2),
                    new MetadataKey(prefix + key2)
                };
            }
        }

        [Theory]
        [MemberData(nameof(WithoutPrefixCases))]
        public void WithoutPrefixReturnsExpectedResult(string prefix, MetadataKey sut, MetadataKey expected)
        {
            var result = sut.WithoutPrefix(prefix);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> WithoutPrefixCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.CustomizeMetadataKey();
                var key = fixture.Create<MetadataKey>();
                yield return new object[]
                {
                    "",
                    key,
                    key
                };

                var prefix = fixture.Create<string>();
                var key2 = fixture.Create<MetadataKey>();
                yield return new object[]
                {
                    prefix,
                    new MetadataKey(prefix + key2),
                    key2
                };

                var upper = prefix.ToUpperInvariant();
                yield return new object[]
                {
                    upper,
                    new MetadataKey(upper + key2),
                    key2
                };

                yield return new object[]
                {
                    upper,
                    new MetadataKey(prefix + key2),
                    key2
                };
            }
        }

        [Fact]
        public void IsComparable()
        {
            Assert.IsAssignableFrom<IComparable<MetadataKey>>(_fixture.Create<MetadataKey>());
        }

        [Theory]
        [MemberData(nameof(CompareCases))]
        public void CompareReturnsExpectedResult(MetadataKey left, MetadataKey right, int expected)
        {
            var result = left.CompareTo(right);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> CompareCases
        {
            get
            {
                yield return new object[] {new MetadataKey("a"), new MetadataKey("b"), -1};
                yield return new object[] {new MetadataKey("b"), new MetadataKey("a"), 1};
                yield return new object[] {new MetadataKey("a"), new MetadataKey("a"), 0};
            }
        }
    }
}
