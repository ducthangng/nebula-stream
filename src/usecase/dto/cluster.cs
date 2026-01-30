/// <summary> 
/// ClusterConfig is the abstract interface for any cloud implementation abstraction
/// </summary>
/// 

namespace NebulaStream.CloudProvider
{
    public enum NClusterStatus
    {
        Creating,
        Active,
        Updating,
        Deleting,
        Failed
    }

    public abstract class ClusterConfig
    {
        public required string ClusterName { get; set; }
        public required string Region { get; set; }
        public required string MachineType { get; set; } // Ví dụ: t3.medium hoặc Standard_DS2_v2
        public required int InitialNodeCount { get; set; }
        public required Dictionary<string, string> Tags { get; set; }

        public ClusterConfig(string ClusterName, string Region, string MachineType, int InitialNodeCount)
        {
            this.ClusterName = ClusterName;
            this.Region = Region;
            this.MachineType = MachineType;
            this.InitialNodeCount = InitialNodeCount;
        }
    }

    public class ClusterResponse
    {
        /// <summary>
        ///  The ID of Cluster on the cloud infa
        /// </summary>
        public required string ClusterId { get; set; }

        /// <summary>
        ///  Cluster Name
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        ///  Endpoint URL so that API server Kubernetes can communicate (ví dụ: https://xyz.eks.amazonaws.com)
        /// </summary>
        public required string ApiEndpoint { get; set; }

        public required NClusterStatus Status { get; set; }

        /// <summary>
        /// Message from the cluster, can be error or info
        /// </summary>
        public string Message { get; set; }

        public required DateTime CreatedAt { get; set; }

        /// <summary>
        ///  Metadata  (Dùng Dictionary để linh hoạt cho từng Cloud)
        /// 
        /// </summary>
        public Dictionary<string, string> ProviderMetadata { get; set; } = new Dictionary<string, string>();
    }
}




