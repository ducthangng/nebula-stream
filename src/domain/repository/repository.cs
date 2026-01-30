using NebulaStream.CloudProvider;


/// <summary>
/// Interface quản lý lưu trữ thông tin Cluster vào Database.
/// </summary>
public interface IClusterRepository
{
    Task SaveClusterAsync(ClusterResponse cluster);
    Task UpdateStatusAsync(string clusterId, NClusterStatus status);
}