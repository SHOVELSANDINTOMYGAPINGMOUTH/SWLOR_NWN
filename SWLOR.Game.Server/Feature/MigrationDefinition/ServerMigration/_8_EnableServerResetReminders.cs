using System.Linq;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.MigrationService;

namespace SWLOR.Game.Server.Feature.MigrationDefinition.ServerMigration
{
    public class _8_EnableServerResetReminders: ServerMigrationBase, IServerMigration
    {
        public int Version => 8;
        public void Migrate()
        {
            var dbPlayers = DB.Players.ToList();
            foreach (var dbPlayer in dbPlayers)
            {
                dbPlayer.Settings.DisplayServerResetReminders = true;
                DB.Set(dbPlayer);
            }
        }
    }
}
