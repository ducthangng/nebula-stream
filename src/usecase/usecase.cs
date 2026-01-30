using System.Threading.Tasks;
using NebulaStream.CloudProvider;

namespace NebulaStream.CloudAbstractions
{
    public interface ICloudOrchestrator
    {
        Task<ClusterResponse> CreateClusterAsync(ClusterConfig config);
        Task<bool> DeleteClusterAsync(string clusterId);
        Task<NClusterStatus> GetStatusAsync(string clusterId);

        // Quản lý Scaling (Node Pools)
        Task UpdateNodeCountAsync(string clusterId, int desiredCount);

        // Quản lý kết nối (Lấy Kubeconfig để deploy app sau này)
        Task<string> GetKubeConfigAsync(string clusterId);

        // Deployment/Management sơ bộ
        Task DeployManifestAsync(string clusterId, string manifestContent);
    }
}