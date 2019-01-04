namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
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
            Assert.Equal(512, BlobName.MaxLength);
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
