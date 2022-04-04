namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Framework;

    public class S3ServerEmbeddedContainer : DockerContainer, IS3Server
    {
        private const int HostPort = 24572;

        public S3ServerEmbeddedContainer()
        {
            Configuration = new MinioContainerConfiguration();
        }

        private static readonly AmazonS3Config S3Config = new AmazonS3Config
        {
            LogMetrics = false,
            DisableLogging = true,
            DisableHostPrefixInjection = true,
            ThrottleRetries = false,
            UseHttp = true,
            UseDualstackEndpoint = false,
            UseAccelerateEndpoint = false,
            EndpointDiscoveryEnabled = false,
            ForcePathStyle = true,
            RegionEndpoint = RegionEndpoint.EUWest1,
            ServiceURL = $"http://localhost:{HostPort}"
        };

        public AmazonS3Client CreateClient()
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(MinioContainerConfiguration.MINIO_ROOT_USER, MinioContainerConfiguration.MINIO_ROOT_PASSWORD),
                S3Config);
        }

        private class MinioContainerConfiguration : DockerContainerConfiguration
        {
            public const string MINIO_ROOT_USER = "AKIAIOSFODNN7EXAMPLE";
            public const string MINIO_ROOT_PASSWORD = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
            public MinioContainerConfiguration()
            {
                Image = new ImageSettings
                {
                    Name = "bitnami/minio"
                };

                Container = new ContainerSettings
                {
                    Name = "blobstore-s3-minio",
                    Command = new [] { "server", "/data", "--console-address ':9001'" },
                    PortBindings = new[]
                    {
                        new PortBinding
                        {
                            HostPort = HostPort,
                            GuestPort = 9000
                        }
                    },
                    EnvironmentVariables = new []
                    {
                        "MINIO_ROOT_USER=" + MINIO_ROOT_USER,
                        "MINIO_ROOT_PASSWORD=" + MINIO_ROOT_PASSWORD
                    }
                };

                WaitUntilAvailable = async attempt =>
                {
                    if (attempt <= 30)
                    {
                        try
                        {
                            using (var client = new AmazonS3Client(new BasicAWSCredentials(MINIO_ROOT_USER, MINIO_ROOT_PASSWORD), S3Config))
                            {
                                await client.ListBucketsAsync();
                            }

                            return TimeSpan.Zero;
                        }
                        catch
                        {
                        }

                        return TimeSpan.FromSeconds(1);
                    }

                    throw new TimeoutException(
                        $"The container {Container.Name} did not become available in a timely fashion.");
                };
            }
        }
    }
}
