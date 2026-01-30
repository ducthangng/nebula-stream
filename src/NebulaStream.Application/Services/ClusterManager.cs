using NebulaStream.CloudAbstractions;
using NebulaStream.Infrastructure.CloudProviders.AWS;
using NebulaStream.Application.DTOs;
using NebulaStream.Core.Entities;

public class ClusterManager
{
    private readonly IKubernetesOrchestration _eksOrchestrator;
    private readonly IClusterRepository _repository;

    private readonly ILogger<ClusterManager> _logger;

    public ClusterManager(
        IKubernetesOrchestration _eksOrchestrator,
        IClusterRepository _repository,
        ILogger<ClusterManager> _logger
    )
    {
        this._eksOrchestrator = _eksOrchestrator;
        this._repository = _repository;
        this._logger = _logger;
    }

    /// <summary>
    /// CreateAndRegisterClusterAsync is the manager of the process.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task<ClusterResponse> CreateAndRegisterClusterAsync(Cluster config)
    {
        var response = await _eksOrchestrator.CreateClusterAsync(config);

        try
        {
            await _repository.SaveClusterAsync(response);
            _logger.LogInformation("Lưu DB thành công cho Cluster: {Id}", response.ClusterId);
        }
        catch (Exception ex)
        {
            // Trong production, nếu lưu DB lỗi bạn cần quyết định: 
            // Có nên xóa cluster vừa tạo trên AWS không? (Rollback)
            _logger.LogError(ex, "Lỗi lưu Database nhưng Cluster đã được tạo trên Cloud.");
        }

        return response;
    }
}