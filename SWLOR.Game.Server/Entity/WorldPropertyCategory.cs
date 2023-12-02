using System.Collections.Generic;
using Redis.OM.Modeling;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(WorldPropertyCategory) })]
    public class WorldPropertyCategory: EntityBase
    {
        public WorldPropertyCategory()
        {
            Items = new Dictionary<string, WorldPropertyItem>();
        }

        [Indexed]
        public string ParentPropertyId { get; set; }

        [Indexed]
        public string Name { get; set; }

        public Dictionary<string, WorldPropertyItem> Items { get; set; }

    }

    public class WorldPropertyItem
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Resref { get; set; }
        public string IconResref { get; set; }
        public int Quantity { get; set; }

        public string Data { get; set; }
    }
}
