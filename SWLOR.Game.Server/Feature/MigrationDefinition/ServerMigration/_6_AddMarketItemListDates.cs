using System;
using System.Linq;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.MigrationService;

namespace SWLOR.Game.Server.Feature.MigrationDefinition.ServerMigration
{
    public class _6_AddMarketItemListDates : IServerMigration
    {
        public int Version => 6;
        public void Migrate()
        {
            var listings = DB.MarketItems.ToList();
            var now = DateTime.UtcNow;
            foreach (var listing in listings)
            {
                listing.DateListed = now;
                DB.Set(listing);
            }
        }
    }
}
