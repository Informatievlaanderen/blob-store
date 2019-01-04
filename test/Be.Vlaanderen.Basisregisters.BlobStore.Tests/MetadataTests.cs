namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Framework;
    using Xunit;

    public class MetadataTests
    {
        private readonly Fixture _fixture;

        public MetadataTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeMetadata();
            _fixture.CustomizeMetadataKey();
        }

        [Fact]
        public void NoneReturnsExpectedResult()
        {
            Assert.Empty(Metadata.None);
        }

        [Fact]
        public void AddHasExpectedResult()
        {
            var metadatum = _fixture.Create<KeyValuePair<MetadataKey, string>>();

            var result = Metadata.None.Add(metadatum);

            Assert.Equal(
                new [] { metadatum },
                result);
        }

        [Fact]
        public void RepeatedAddHasExpectedResult()
        {
            var metadata = _fixture.CreateMany<KeyValuePair<MetadataKey, string>>(new Random().Next(1, 10)).ToArray();

            var result = metadata.Aggregate(
                Metadata.None,
                (data, metadatum) => data.Add(metadatum));

            Assert.Equal(
                metadata,
                result);
        }


        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
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
            ).Verify(typeof(Metadata));
        }
    }
}
