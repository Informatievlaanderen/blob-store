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

    public class BlobNameTests
    {
        private readonly Fixture _fixture;

        public BlobNameTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeBlobName();
        }

        [Fact]
        public void MaximumLengthReturnsExpectedValue()
        {
            Assert.Equal(128, BlobName.MaxLength);
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.Customize<string>(customization =>
                customization.FromFactory(generator =>
                    new string(
                        (char) generator.Next(97, 123), // a-z
                        generator.Next(1, BlobName.MaxLength + 1)
                    )
                ));
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
                new EquatableEqualsOtherAssertion(_fixture),
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
            ).Verify(typeof(BlobName));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = new string(
                (char) new Random().Next(97, 123), // a-z
                new Random().Next(1, BlobName.MaxLength + 1)
            );
            var sut = new BlobName(value);

            Assert.Equal(value, sut.ToString());
        }

        [Fact]
        public void ValueCanNotBeLongerThan512Chars()
        {
            const int length = BlobName.MaxLength + 1;

            var value = new string((char) new Random().Next(97, 123), length);
            Assert.Throws<ArgumentException>(() => new BlobName(value));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void ValueIsWellformed(string value)
        {
            Assert.NotEqual(default, new BlobName(value));
        }

        public static IEnumerable<object[]> WellformedCases
        {
            get { return WellformedBlobNames.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> WellformedBlobNames
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
                        .Concat(new[] { '!', '/', '-', '_', '.', '*', '\'', '(', ')' })
                        .ToArray();

                var fixture = new Fixture();
                fixture.Customizations.Add(
                    new FiniteSequenceGenerator<char>(acceptableCharacters));

                yield return new string(fixture.CreateMany<char>(random.Next(1, BlobName.MaxLength + 1)).ToArray());
            }
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void ValueIsNotWellformed(string value)
        {
            Assert.Throws<ArgumentException>(() => new BlobName(value));
        }

        public static IEnumerable<object[]> NotWellformedCases
        {
            get { return NotWellformedBlobNames.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> NotWellformedBlobNames
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
                        .Concat(new[] { '!', '/', '-', '_', '.', '*', '\'', '(', ')' })
                        .ToArray();

                yield return new string(
                    generator
                        .Where(candidate => !Array.Exists(acceptableCharacters, character => character == candidate))
                        .Take(3)
                        .ToArray()
                );
            }
        }

        [Fact]
        public void PrependReturnsExpectedResult()
        {
            _fixture.CustomizeBlobName(BlobName.MaxLength / 2);
            var prefix = _fixture.Create<BlobName>();
            var sut = _fixture.Create<BlobName>();

            var result = sut.Prepend(prefix);

            Assert.Equal(new BlobName(prefix + sut), result);
        }

        [Fact]
        public void PrependThrowsWhenResultTooLong()
        {
            var prefix = new BlobName(new string('a', BlobName.MaxLength));
            var sut = _fixture.Create<BlobName>();

            Assert.Throws<ArgumentException>(() => sut.Prepend(prefix));
        }

        [Fact]
        public void AppendReturnsExpectedResult()
        {
            _fixture.CustomizeBlobName(BlobName.MaxLength / 2);
            var suffix = _fixture.Create<BlobName>();
            var sut = _fixture.Create<BlobName>();

            var result = sut.Append(suffix);

            Assert.Equal(new BlobName(sut + suffix), result);
        }

        [Fact]
        public void AppendThrowsWhenResultTooLong()
        {
            var suffix = new BlobName(new string('a', BlobName.MaxLength));
            var sut = _fixture.Create<BlobName>();

            Assert.Throws<ArgumentException>(() => sut.Append(suffix));
        }
    }
}
