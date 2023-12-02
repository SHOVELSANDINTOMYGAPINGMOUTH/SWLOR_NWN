using Redis.OM.Modeling;
using System.Collections.Generic;
using System.Numerics;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(ModuleCache) })]
    public class ModuleCache: EntityBase
    {
        public ModuleCache()
        {
            Id = "SWLOR_CACHE";
            WalkmeshesByArea = new Dictionary<string, List<Vector3>>();
        }

        public int LastModuleMTime { get; set; }
        public Dictionary<string, List<Vector3>> WalkmeshesByArea { get; set; }
        public Dictionary<string, string> ItemNamesByResref { get; set; }
    }
}
