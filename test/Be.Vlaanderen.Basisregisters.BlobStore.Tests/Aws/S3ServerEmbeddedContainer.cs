namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
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
            ServiceURL = $"http://localhost:{HostPort}"
        };

        public AmazonS3Client CreateClient()
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(MinioContainerConfiguration.ACCESS_KEY, MinioContainerConfiguration.SECRET_KEY),
                S3Config);
        }

        private class MinioContainerConfiguration : DockerContainerConfiguration
        {
            public const string ACCESS_KEY = "AKIAIOSFODNN7EXAMPLE";
            public const string SECRET_KEY = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
            public MinioContainerConfiguration()
            {
                Image = new ImageSettings
                {
                    Name = "minio/minio"
                };

                Container = new ContainerSettings
                {
                    Name = "blobstore-s3-minio",
                    Command = new [] { "server", "/data" },
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
                        "MINIO_ACCESS_KEY=" + ACCESS_KEY,
                        "MINIO_SECRET_KEY=" + SECRET_KEY
                    }
                };

                WaitUntilAvailable = async attempt =>
                {
                    if (attempt <= 30)
                    {
                        try
                        {
                            using (var client = new AmazonS3Client(new BasicAWSCredentials(ACCESS_KEY, SECRET_KEY), S3Config))
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
