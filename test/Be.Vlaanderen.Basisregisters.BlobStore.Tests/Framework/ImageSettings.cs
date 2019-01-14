namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    public class ImageSettings
    {
        public string Registry { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; } = "latest";

        public string TagQualifiedName => Name + ":" + Tag;
        public string RegistryQualifiedName => Registry + "/" + Name;
        public string FullyQualifiedName => Registry + "/" + TagQualifiedName;
    }
}