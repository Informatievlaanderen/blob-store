namespace Be.Vlaanderen.Basisregisters.BlobStore.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileBlobClientTests : BlobClientTests, IDisposable
    {
        private readonly DirectoryInfo _temporaryDirectory;

        public FileBlobClientTests()
        {
            var tempPathDirectory = new DirectoryInfo(Path.GetTempPath());
            var name =
                $"D{Process.GetCurrentProcess().Id}_{Thread.CurrentThread.ManagedThreadId}_{DateTimeOffset.UtcNow.Ticks}";
            _temporaryDirectory = tempPathDirectory.CreateSubdirectory(name);
        }
        protected override Task<IBlobClient> CreateClient()
        {
            return Task.FromResult((IBlobClient)new FileBlobClient(_temporaryDirectory));
        }

        public void Dispose()
        {
            foreach (var file in _temporaryDirectory.EnumerateFiles())
            {
                file.Delete();
            }
            _temporaryDirectory.Delete();
        }
    }
}
