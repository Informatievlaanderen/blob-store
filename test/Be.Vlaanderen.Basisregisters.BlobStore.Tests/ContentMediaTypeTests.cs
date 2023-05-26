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

    public class ContentMediaTypeTests
    {
        private readonly Fixture _fixture;

        public ContentMediaTypeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeContentMediaType();
        }

        [Fact]
        public void MaximumLengthReturnsExpectedValue()
        {
            Assert.Equal(64, ContentMediaType.MaxLength);
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.CustomizeContentMediaTypeString();
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
            ).Verify(typeof(ContentMediaType));
        }

        [Fact]
        public void ParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaType.Parse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            Assert.NotEqual(default, ContentMediaType.Parse(value));
        }

        public static IEnumerable<object[]> WellformedCases
        {
            get { return WellformedMediaTypes.Select(@case => new object[] { @case }); }
        }

        public static IEnumerable<string> WellformedMediaTypes
        {
            get
            {
                var knownMediaTypes = new []
                {
                    "application",
                    "audio",
                    "font",
                    "example",
                    "image",
                    "message",
                    "model",
                    "multipart",
                    "text",
                    "video",
                    "binary"
                };
                foreach (var knownMediaType in knownMediaTypes)
                {
                    yield return knownMediaType;
                }
                // custom
                yield return "X-application";
            }
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            Assert.Throws<FormatException>(() => ContentMediaType.Parse(value));
        }

        public static IEnumerable<object[]> NotWellformedCases
        {
            get { return NotWellformedMediaTypes.Select(@case => new object[] { @case }); }
        }
        public static IEnumerable<string> NotWellformedMediaTypes
        {
            get
            {
                yield return "";
                yield return "unknown";
                yield return "X-";
                yield return "X-" + new string('a', ContentMediaType.MaxLength - 1);
                // space
                yield return "X- ";
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
                    yield return "X-" + controlCharacter;
                }
                // unacceptable
                var unacceptableCharacters = new []
                {
                    '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[', ']', '?', '.', '='
                };
                foreach (var unacceptableCharacter in unacceptableCharacters)
                {
                    yield return "X-" + unacceptableCharacter;
                }
            }
        }

        [Fact]
        public void TryParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaType.TryParse(null, out _));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentMediaType.TryParse(value, out var parsed);
            Assert.True(result);
            Assert.Equal(value, parsed.ToString());
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentMediaType.TryParse(value, out var parsed);
            Assert.False(result);
            Assert.Equal(default, parsed);
        }

        [Fact]
        public void CanParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentMediaType.CanParse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentMediaType.CanParse(value);
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentMediaType.CanParse(value);
            Assert.False(result);
        }
    }
}
