using Redis.OM.Modeling;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(PlayerNote) })]
    public class PlayerNote: EntityBase
    {
        [Indexed]
        public string PlayerId { get; set; }
        [Searchable]
        public string Name { get; set; }
        public string Text { get; set; }

        [Indexed]
        public bool IsDMNote { get; set; }
        [Searchable]
        public string DMCreatorName { get; set; }
        [Searchable]
        public string DMCreatorCDKey { get; set; }

        public PlayerNote()
        {
            IsDMNote = false;
            DMCreatorName = string.Empty;
            DMCreatorCDKey = string.Empty;
        }
    }
}
