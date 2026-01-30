/// <summary> 
/// ClusterConfig is the abstract interface for any cloud implementation abstraction
/// </summary>
namespace NebulaStream.Core.Entities
{
    public abstract class Cluster
    {
        public required string ClusterName { get; set; }
        public required string Region { get; set; }
        public required string MachineType { get; set; } // Ví dụ: t3.medium hoặc Standard_DS2_v2
        public required int InitialNodeCount { get; set; }
        public required Dictionary<string, string> Tags { get; set; }

        public Cluster(string ClusterName, string Region, string MachineType, int InitialNodeCount)
        {
            this.ClusterName = ClusterName;
            this.Region = Region;
            this.MachineType = MachineType;
            this.InitialNodeCount = InitialNodeCount;
        }
    }
}




