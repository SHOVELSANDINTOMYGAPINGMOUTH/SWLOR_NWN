using Redis.OM.Modeling;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(ServerConfiguration) })]
    public class ServerConfiguration: EntityBase
    {
        public ServerConfiguration()
        {
            Id = "SWLOR_CONFIG";
            MigrationVersion = 0;
        }

        [Indexed]
        public int MigrationVersion { get; set; }
    }
}
