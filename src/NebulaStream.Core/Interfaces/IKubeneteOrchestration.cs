using NebulaStream.Application.DTOs;
using NebulaStream.Core.Entities;
using NebulaStream.Core.Enums;

namespace NebulaStream.CloudAbstractions
{
    public interface IKubernetesOrchestration
    {
        Task<ClusterResponse> CreateClusterAsync(Cluster clusterConfig);
        Task<bool> DeleteClusterAsync(string clusterId);
        Task<ClusterStatus> GetStatusAsync(string clusterId);

        // Quản lý Scaling (Node Pools)
        Task UpdateNodeCountAsync(string clusterId, int desiredCount);

        // Quản lý kết nối (Lấy Kubeconfig để deploy app sau này)
        Task<string> GetKubeConfigAsync(string clusterId);

        // Deployment/Management sơ bộ
        Task DeployManifestAsync(string clusterId, string manifestContent);
    }
}