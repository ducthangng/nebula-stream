using Amazon;
using Amazon.EKS;
using Amazon.EKS.Model;
using AwsStatus = Amazon.EKS.ClusterStatus;

using Microsoft.Extensions.Logging;
using NebulaStream.CloudAbstractions;
using NebulaStream.CloudProvider;


namespace NebulaStream.AWSAbstractions
{

    public class EksConfig : ClusterConfig
    {

        // Polymorphism: only AWS_EKS has this
        public required string[] SubnetIds { get; set; }
        public required string RoleArn { get; set; }
        public string[]? SecurityGroupIds { get; set; }

        public EksConfig(string ClusterName, string Region, string MachineType,
            int InitialNodeCount, string roleArn, string[] subnetIds) :
            base(ClusterName, Region, MachineType, InitialNodeCount)
        {
            RoleArn = roleArn;
            SubnetIds = subnetIds;
        }
    }

    public class AwsEksOrchestrator : ICloudOrchestrator
    {
        /// <summary>
        ///  _eksClient implements IAmazonEKS to manage the EKS.
        /// </summary>
        private readonly IAmazonEKS _eksClient;

        /// <summary>
        /// ILogger is from the Logging extension of Microsoft, to allow you to log the info out.
        /// </summary>
        private readonly ILogger<AwsEksOrchestrator> _logger;

        /// <summary>
        /// Constructor for the class, the params are required.
        /// </summary>
        /// <param name="eksClient"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AwsEksOrchestrator(IAmazonEKS eksClient, ILogger<AwsEksOrchestrator> logger)
        {
            _eksClient = eksClient ?? throw new ArgumentNullException(nameof(eksClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private NClusterStatus MapAwsStatus(AwsStatus awsStatus) // Dùng biệt danh ở đây
        {
            if (awsStatus == AwsStatus.ACTIVE) return NClusterStatus.Active;
            if (awsStatus == AwsStatus.CREATING) return NClusterStatus.Creating;
            if (awsStatus == AwsStatus.DELETING) return NClusterStatus.Deleting;
            if (awsStatus == AwsStatus.FAILED) return NClusterStatus.Failed;

            // default
            return NClusterStatus.Updating;
        }

        public async Task<ClusterResponse> CreateClusterAsync(ClusterConfig config)
        {
            // cast to EKS config: Implicit Upcasting
            // ClusterConfig is the children of config, so it can be pushed into the father
            if (config is not EksConfig eksConfig)
            {
                throw new ArgumentException("Invalid config for AWS EKS: ", nameof(config));
            }

            try
            {
                _logger.LogInformation("Requesting for EKS Cluster: {Name} ...", eksConfig.ClusterName);

                var request = new CreateClusterRequest
                {
                    Name = eksConfig.ClusterName,
                    RoleArn = eksConfig.RoleArn,
                    ResourcesVpcConfig = new VpcConfigRequest
                    {
                        SubnetIds = eksConfig.SubnetIds.ToList(),
                        SecurityGroupIds = eksConfig.SecurityGroupIds?.ToList()
                    },
                    Version = "1.27"
                };

                var response = await _eksClient.CreateClusterAsync(request);

                _logger.LogInformation("Request for cluster {Name} has been sent", eksConfig.ClusterName);

                return new ClusterResponse
                {
                    ClusterId = response.Cluster.Arn,
                    Name = response.Cluster.Name,
                    ApiEndpoint = response.Cluster.Endpoint,
                    Status = MapAwsStatus(response.Cluster.Status),
                    CreatedAt = response.Cluster.CreatedAt ?? DateTime.UtcNow,
                };
            }
            catch (AmazonEKSException ex)
            {
                _logger.LogError(ex, "Error when connecting to cluster {Name}", eksConfig.ClusterName);
                throw; // push error to the outer part
            }
        }

        public Task<bool> DeleteClusterAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        public Task DeployManifestAsync(string clusterId, string manifestContent)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetKubeConfigAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        public Task<NClusterStatus> GetStatusAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNodeCountAsync(string clusterId, int desiredCount)
        {
            throw new NotImplementedException();
        }
    }
}