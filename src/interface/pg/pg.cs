using NebulaStream.CloudProvider;

public class PostgresClusterRepository : IClusterRepository
{
    private readonly string _connectionString;

    public PostgresClusterRepository(string connectionString)
    {
        this._connectionString = connectionString;
    }

    public async Task SaveClusterAsync(ClusterResponse cluster)
    {

    }

    Task IClusterRepository.SaveClusterAsync(ClusterResponse cluster)
    {
        throw new NotImplementedException();
    }

    Task IClusterRepository.UpdateStatusAsync(string clusterId, NClusterStatus status)
    {
        throw new NotImplementedException();
    }
}