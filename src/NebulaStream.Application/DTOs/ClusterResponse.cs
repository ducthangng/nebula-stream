
using NebulaStream.Core.Enums;

namespace NebulaStream.Application.DTOs
{
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

        public required ClusterStatus Status { get; set; }

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
};