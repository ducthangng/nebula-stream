using Amazon.EKS;
using Amazon.EKS.Model;
using NebulaStream.Application.DTOs;
using NebulaStream.CloudAbstractions;
using AwsStatus = Amazon.EKS.ClusterStatus;
using Cluster = NebulaStream.Core.Entities.Cluster;
using ClusterStatus = NebulaStream.Core.Enums.ClusterStatus;
using Microsoft.Extensions.Logging;



namespace NebulaStream.Infrastructure.CloudProviders.AWS
{
    public class EksClusterOrchestrator : IKubernetesOrchestration
    {
        /// <summary>
        ///  _eksClient implements IAmazonEKS to manage the EKS.
        /// </summary>
        private readonly IAmazonEKS _eksClient;

        /// <summary>
        /// ILogger is from the Logging extension of Microsoft, to allow you to log the info out.
        /// </summary>
        private readonly ILogger<EksClusterOrchestrator> _logger;

        /// <summary>
        /// Constructor for the class, the params are required.
        /// </summary>
        /// <param name="eksClient"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public EksClusterOrchestrator(IAmazonEKS eksClient, ILogger<EksClusterOrchestrator> logger)
        {
            _eksClient = eksClient ?? throw new ArgumentNullException(nameof(eksClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private ClusterStatus MapAwsStatus(AwsStatus awsStatus) // Dùng biệt danh ở đây
        {
            if (awsStatus == AwsStatus.ACTIVE) return ClusterStatus.Active;
            if (awsStatus == AwsStatus.CREATING) return ClusterStatus.Creating;
            if (awsStatus == AwsStatus.DELETING) return ClusterStatus.Deleting;
            if (awsStatus == AwsStatus.FAILED) return ClusterStatus.Failed;

            // default
            return ClusterStatus.Updating;
        }

        public async Task<ClusterResponse> CreateClusterAsync(Cluster config)
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

        Task<ClusterResponse> IKubernetesOrchestration.CreateClusterAsync(Amazon.EKS.Model.Cluster clusterConfig)
        {
            throw new NotImplementedException();
        }

        Task<bool> IKubernetesOrchestration.DeleteClusterAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        Task<AwsStatus> IKubernetesOrchestration.GetStatusAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        Task IKubernetesOrchestration.UpdateNodeCountAsync(string clusterId, int desiredCount)
        {
            throw new NotImplementedException();
        }

        Task<string> IKubernetesOrchestration.GetKubeConfigAsync(string clusterId)
        {
            throw new NotImplementedException();
        }

        Task IKubernetesOrchestration.DeployManifestAsync(string clusterId, string manifestContent)
        {
            throw new NotImplementedException();
        }
    }
}