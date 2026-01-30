using NebulaStream.Application.DTOs;
using NebulaStream.Core.Enums;

/// <summary>
/// Interface quản lý lưu trữ thông tin Cluster vào Database.
/// </summary>
public interface IClusterRepository
{
    Task SaveClusterAsync(ClusterResponse cluster);
    Task UpdateStatusAsync(string clusterId, ClusterStatus status);
}
