using Redis.OM.Modeling;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(PlayerBan) })]
    public class PlayerBan: EntityBase
    {
        public string CDKey { get; set; }
        public string Reason { get; set; }
    }
}
