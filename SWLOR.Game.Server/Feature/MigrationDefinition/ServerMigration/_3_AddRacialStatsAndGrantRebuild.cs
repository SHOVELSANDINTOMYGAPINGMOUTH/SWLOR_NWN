using System.Linq;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Feature.MigrationDefinition.ServerMigration
{
    public class _3_AddRacialStatsAndGrantRebuild: ServerMigrationBase
    {
        public int Version => 3;
        public void Migrate()
        {
            var players = DB.Players.ToList();

            foreach (var player in players)
            {
                player.RacialStat = AbilityType.Invalid;

                DB.Set(player);
            }
        }
    }
}
