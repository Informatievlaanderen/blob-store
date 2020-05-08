namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;
    using Framework;
    using Xunit;

    public class ContentTypeTests
    {
        private readonly Fixture _fixture;

        public ContentTypeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeContentMediaType();
            _fixture.CustomizeContentMediaSubtype();
            _fixture.CustomizeContentType();
        }

        [Fact]
        public void MaximumLengthReturnsExpectedValue()
        {
            Assert.Equal(129, ContentType.MaxLength);
        }

        [Fact]
        public void VerifyBehavior()
        {
            var customizedString = new Fixture();
            customizedString.CustomizeContentTypeString();
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
            ).Verify(typeof(ContentType));
        }

        [Fact]
        public void ParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentType.Parse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            Assert.NotEqual(default, ContentType.Parse(value));
        }

        public static IEnumerable<object[]> WellformedCases
        {
            get
            {
                var random = new Random();

                var cases =
                    from mediaType in ContentMediaTypeTests.WellformedMediaTypes.OrderBy(_ => random.Next()).Take(10)
                    from mediaSubtype in ContentMediaSubtypeTests.WellformedMediaSubtypes.OrderBy(_ => random.Next()).Take(10)
                    select mediaType + "/" + mediaSubtype;

                foreach(var @case in cases.Distinct())
                {
                    yield return new object[] { @case };
                }
            }
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void ParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            Assert.Throws<FormatException>(() => ContentType.Parse(value));
        }

        public static IEnumerable<object[]> NotWellformedCases
        {
            get
            {
                yield return new object[] { "/" };

                var random = new Random();

                var cases =
                    from mediaType in ContentMediaTypeTests.NotWellformedMediaTypes.OrderBy(_ => random.Next()).Take(10)
                    from mediaSubtype in ContentMediaSubtypeTests.NotWellformedMediaSubtypes.OrderBy(_ => random.Next()).Take(10)
                    select mediaType + "/" + mediaSubtype;

                foreach(var @case in cases.Distinct())
                {
                    yield return new object[] { @case };
                }

                foreach (var mediaType in ContentMediaTypeTests.WellformedMediaTypes.OrderBy(_ => random.Next()).Take(10))
                {
                    yield return new object[] { mediaType + "/" };
                    yield return new object[] { mediaType };
                }

                foreach (var mediaSubtype in ContentMediaSubtypeTests.WellformedMediaSubtypes.OrderBy(_ => random.Next()).Take(10))
                {
                    yield return new object[] { "/" + mediaSubtype };
                    yield return new object[] { mediaSubtype };
                }
            }
        }

        [Fact]
        public void TryParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentType.TryParse(null, out _));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentType.TryParse(value, out var parsed);
            Assert.True(result);
            Assert.Equal(value, parsed.ToString());
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void TryParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentType.TryParse(value, out var parsed);
            Assert.False(result);
            Assert.Equal(default, parsed);
        }

        [Fact]
        public void CanParseValueCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ContentType.CanParse(null));
        }

        [Theory]
        [MemberData(nameof(WellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsWellformed(string value)
        {
            var result = ContentType.CanParse(value);
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(NotWellformedCases))]
        public void CanParseReturnsExpectedResultWhenValueIsNotWellformed(string value)
        {
            var result = ContentType.CanParse(value);
            Assert.False(result);
        }
    }
}
