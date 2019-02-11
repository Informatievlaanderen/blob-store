namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    public class ContainerSettings
    {
        public string[] Command { get; set; } = new string[0];

        public string Name { get; set; }

        public PortBinding[] PortBindings { get; set; } = new PortBinding[0];

        public string[] EnvironmentVariables { get; set; } = new string[0];

        public bool StopContainer { get; set; } = true;

        public bool RemoveContainer { get; set; } = true;
    }
}
