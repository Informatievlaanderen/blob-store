namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;

    public class S3ServerComposedContainer : IS3Server
    {
        private int _hostPort;
        private string _rootUser;
        private string _rootPassword;

        public S3ServerComposedContainer()
        {
            if (Environment.GetEnvironmentVariable("MINIO_PORT") == null)
            {
                throw new Exception("The MINIO_PORT environment variable is missing.");
            }

            if (Environment.GetEnvironmentVariable("MINIO_ROOT_USER") == null)
            {
                throw new Exception("The MINIO_ROOT_USER environment variable is missing.");
            }

            if (Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD") == null)
            {
                throw new Exception("The MINIO_ROOT_PASSWORD environment variable is missing.");
            }

            _hostPort = int.Parse(Environment.GetEnvironmentVariable("MINIO_PORT"));
            _rootUser = Environment.GetEnvironmentVariable("MINIO_ROOT_USER");
            _rootPassword = Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD");
        }

        public async Task InitializeAsync()
        {
            async Task<TimeSpan> WaitUntilAvailable(int current)
            {
                if (current <= 30)
                {
                    try
                    {
                        using (var client = new AmazonS3Client(new BasicAWSCredentials(_rootUser, _rootPassword), CreateClientConfig()))
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
                    $"The minio container did not become available in a timely fashion.");
            }

            var attempt = 0;
            var result = await WaitUntilAvailable(attempt++);
            while (result > TimeSpan.Zero)
            {
                await Task.Delay(result);
                result = await WaitUntilAvailable(attempt++);
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public AmazonS3Client CreateClient()
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(_rootUser, _rootPassword),
                CreateClientConfig());
        }

        private AmazonS3Config CreateClientConfig()
        {
            return new AmazonS3Config
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
                ServiceURL = $"http://localhost:{_hostPort}"
            };
        }
    }
}
