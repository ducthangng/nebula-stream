using NebulaStream.Core.Entities;

namespace NebulaStream.Infrastructure.CloudProviders.AWS;

public class EksConfig : Cluster
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