namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public abstract class BlobClientTests
    {
        private readonly Fixture _fixture;

        public BlobClientTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeBlobName();
            _fixture.CustomizeMetadata();
            _fixture.CustomizeContentType();
        }

        [Fact]
        public async Task GetNonExistingBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();

            var result = await sut.GetBlobAsync(_fixture.Create<BlobName>());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetExistingBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }

            var result = await sut.GetBlobAsync(name);

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(metadata, result.Metadata);
            Assert.Equal(contentType, result.ContentType);
            using(var openedStream = await result.OpenAsync())
            {
                using(var outputStream = new MemoryStream())
                {
                    await openedStream.CopyToAsync(outputStream);
                    Assert.Equal(bytes, outputStream.ToArray());
                }
            }
        }

        [Fact]
        public async Task OpenExistingBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }
            var blob = await sut.GetBlobAsync(name);

            using(var result = await blob.OpenAsync())
            {
                Assert.NotNull(result);
                using(var outputStream = new MemoryStream())
                {
                    await result.CopyToAsync(outputStream);
                    Assert.Equal(bytes, outputStream.ToArray());
                }
            }
        }

        [Fact]
        public async Task OpenExistingBlobAsZipArchiveHasExpectedBehavior()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            using(var inputStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(inputStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("content.txt");
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        await writer.WriteLineAsync("lorum.ipsum");
                        await writer.FlushAsync();
                    }
                    await inputStream.FlushAsync();
                }
            
                inputStream.Position = 0;
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
                var blob = await sut.GetBlobAsync(name);

                using(var result = await blob.OpenAsync())
                {
                    Assert.NotNull(result);
                    using (var archive = new ZipArchive(result, ZipArchiveMode.Read))
                    {
                        var entry = archive.GetEntry("content.txt");
                        using (var entryStream = entry.Open())
                        using (var reader = new StreamReader(entryStream))
                        {
                            var line = await reader.ReadLineAsync();
                            Assert.Equal("lorum.ipsum", line);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task OpenDeletedBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }
            var blob = await sut.GetBlobAsync(name);
            await sut.DeleteBlobAsync(name);

            await Assert.ThrowsAsync<BlobNotFoundException>(() => blob.OpenAsync());
        }

        [Fact]
        public async Task CreateNonExistingBlobHasExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }

            var result = await sut.GetBlobAsync(name);
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(metadata, result.Metadata);
            Assert.Equal(contentType, result.ContentType);
            using(var openedStream = await result.OpenAsync())
            {
                using(var outputStream = new MemoryStream())
                {
                    await openedStream.CopyToAsync(outputStream);
                    Assert.Equal(bytes, outputStream.ToArray());
                }
            }
        }

        [Fact]
        public async Task CreateExistingBlobHasExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            
                var exception = await Assert.ThrowsAsync<BlobAlreadyExistsException>(
                    () => sut.CreateBlobAsync(name, metadata, contentType, inputStream));
                Assert.Equal(name, exception.Name);
            }
        }

        [Fact]
        public async Task DeleteNonExistingBlobReturnsExpectedResult()
        {
            var name = _fixture.Create<BlobName>();
            var sut = await CreateClient();

            await sut.DeleteBlobAsync(name);

            Assert.Null(await sut.GetBlobAsync(name));
        }

        [Fact]
        public async Task DeleteExistingBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }

            await sut.DeleteBlobAsync(name);

            Assert.Null(await sut.GetBlobAsync(name));
        }

        [Fact]
        public async Task DeleteDeletedBlobReturnsExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }
            await sut.DeleteBlobAsync(name);

            await sut.DeleteBlobAsync(name);

            Assert.Null(await sut.GetBlobAsync(name));
        }

        [Fact]
        public async Task BlobExistsForExistingBlobHasExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();
            var metadata = _fixture.Create<Metadata>();
            var contentType = _fixture.Create<ContentType>();
            var bytes = _fixture.CreateMany<byte>(new Random().Next(1, 100)).ToArray();
            using(var inputStream = new MemoryStream(bytes))
            {
                await sut.CreateBlobAsync(name, metadata, contentType, inputStream);
            }

            var result = await sut.BlobExistsAsync(name);
            Assert.True(result);
        }

        [Fact]
        public async Task BlobExistsForNonExistingBlobHasExpectedResult()
        {
            var sut = await CreateClient();
            var name = _fixture.Create<BlobName>();

            var result = await sut.BlobExistsAsync(name);
            Assert.False(result);
        }

        protected abstract Task<IBlobClient> CreateClient();
    }
}
