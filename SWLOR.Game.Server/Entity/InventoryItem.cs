using Redis.OM.Modeling;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(InventoryItem) })]
    public class InventoryItem : EntityBase
    {
        [Indexed]
        public string StorageId { get; set; }
        [Indexed]
        public string PlayerId { get; set; }
        [Searchable]
        public string Name { get; set; }
        [Searchable]
        public string Tag { get; set; }
        [Searchable]
        public string Resref { get; set; }
        public int Quantity { get; set; }
        public string Data { get; set; }
        public string IconResref { get; set; }
    }
}
