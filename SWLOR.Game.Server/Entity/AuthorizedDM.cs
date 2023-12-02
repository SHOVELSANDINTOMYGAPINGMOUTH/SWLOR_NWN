using Redis.OM.Modeling;
using SWLOR.Game.Server.Enumeration;

namespace SWLOR.Game.Server.Entity
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(AuthorizedDM) })]
    public class AuthorizedDM: EntityBase
    {
        [Searchable]
        public string Name { get; set; }
        [Searchable]
        public string CDKey { get; set; }
        [Indexed]
        public AuthorizationLevel Authorization { get; set; }
    }
}
