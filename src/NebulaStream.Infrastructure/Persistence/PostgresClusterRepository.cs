using Dapper;
using Npgsql;
using NebulaStream.Application.DTOs;
using NebulaStream.Core.Enums;

public class PostgresClusterRepository : IClusterRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresClusterRepository> _logger;

    public PostgresClusterRepository(string connectionString, ILogger<PostgresClusterRepository> _logger)
    {
        this._connectionString = connectionString;
        this._logger = _logger;
    }

    public async Task SaveClusterAsync(ClusterResponse cluster)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var query = @"
            INSERT INTO nebula_clusters (id, cluster_name, api_endpoint, status, created_at)
            VALUES (@ClusterId, @Name, @ApiEndpoint, @Status, @CreatedAt)
            ON CONFLICT (id) DO UPDATE SET status = @Status;";

        // Dapper tự động map các thuộc tính của 'cluster' vào các @parameter
        await connection.ExecuteAsync(query, new
        {
            cluster.ClusterId,
            cluster.Name,
            cluster.ApiEndpoint,
            Status = cluster.Status.ToString(), // Lưu enum dưới dạng string cho dễ đọc
            cluster.CreatedAt
        });

        _logger.LogInformation("okkk");
    }

    Task IClusterRepository.SaveClusterAsync(ClusterResponse cluster)
    {
        throw new NotImplementedException();
    }

    Task IClusterRepository.UpdateStatusAsync(string clusterId, ClusterStatus status)
    {
        throw new NotImplementedException();
    }
}