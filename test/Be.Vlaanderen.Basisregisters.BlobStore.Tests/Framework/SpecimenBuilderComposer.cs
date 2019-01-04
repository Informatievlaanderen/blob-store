namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using AutoFixture.Kernel;

    internal static class SpecimenBuilderComposer
    {
        internal static object CreateAnonymous(this ISpecimenBuilder builder, object request)
        {
            return new SpecimenContext(builder).Resolve(request);
        }
    }
}
