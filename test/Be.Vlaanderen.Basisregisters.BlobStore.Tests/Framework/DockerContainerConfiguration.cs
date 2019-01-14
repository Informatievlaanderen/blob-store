namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Threading.Tasks;

    public class DockerContainerConfiguration
    {
        public ImageSettings Image { get; set; }

        public ContainerSettings Container { get; set; }

        public Func<int, Task<TimeSpan>> WaitUntilAvailable { get; set; } = attempts => Task.FromResult(TimeSpan.Zero);
    }
}