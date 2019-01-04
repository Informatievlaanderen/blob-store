namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Framework;
    using Xunit;

    public class ContentMediaSubtypeTests
    {
        private readonly Fixture _fixture;

        public ContentMediaSubtypeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeContentMediaSubtype();
        }

        [Fact]
        public void MaximumLengthReturnsExpectedValue()
        {
            Assert.Equal(64, ContentMediaSubtype.MaxLength);
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.CustomizeContentMediaSubtypeString();
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(
                    _fixture,
                    new CompositeBehaviorExpectation(
                        new NullReferenceBehaviorExpectation()
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
            ).Verify(typeof(ContentMediaSubtype));
        }

        [Fact]
        public void ParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaSubtype.Parse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            Assert.NotEqual(default, ContentMediaSubtype.Parse(value));
        }

        public static IEnumerable<object[]> WellformedCases
        {
            get { return WellformedMediaSubtypes.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> WellformedMediaSubtypes
        {
            get
            {
                var random = new Random();
                yield return new string((char)random.Next(97, 123), random.Next(1, ContentMediaSubtype.MaxLength + 1));
            }
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            Assert.Throws<FormatException>(() => ContentMediaSubtype.Parse(value));
        }

        public static IEnumerable<object[]> NotWellformedCases
        {
            get { return NotWellformedMediaSubtypes.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> NotWellformedMediaSubtypes
        {
            get
            {
                yield return "";
                yield return new string('a', ContentMediaSubtype.MaxLength + 1);
                // space
                yield return " ";
                // control
                var controlCharacters =
                    Enumerable
                        .Range(0, 32)
                        .Select(value => (char) value)
                        .Concat(new[] {(char) 127})
                        .Concat(
                            Enumerable
                                .Range(128, 32)
                                .Select(value => (char) value));
                foreach (var controlCharacter in controlCharacters)
                {
                    yield return controlCharacter.ToString();
                }
                // unacceptable
                var unacceptableCharacters = new []
                {
                    '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[', ']', '?', '.', '='
                };
                foreach (var unacceptableCharacter in unacceptableCharacters)
                {
                    yield return unacceptableCharacter.ToString();
                }
            }
        }

        [Fact]
        public void TryParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaSubtype.TryParse(null, out _));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentMediaSubtype.TryParse(value, out var parsed);
            Assert.True(result);
            Assert.Equal(value, parsed.ToString());
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentMediaSubtype.TryParse(value, out var parsed);
            Assert.False(result);
            Assert.Equal(default, parsed);
        }

        [Fact]
        public void CanParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaSubtype.CanParse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentMediaSubtype.CanParse(value);
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentMediaSubtype.CanParse(value);
            Assert.False(result);
        }
    }
}
