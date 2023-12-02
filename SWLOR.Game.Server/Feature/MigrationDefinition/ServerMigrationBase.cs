using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.CurrencyService;
using SWLOR.Game.Server.Service.LogService;
using SWLOR.Game.Server.Service.PerkService;
using System.Collections.Generic;
using System.Linq;

namespace SWLOR.Game.Server.Feature.MigrationDefinition
{
    public abstract class ServerMigrationBase
    {
        protected void GrantRebuildTokenToAllPlayers()
        {
            var dbPlayers = DB.Players.ToList();
            foreach (var dbPlayer in dbPlayers)
            {
                if (!dbPlayer.Currencies.ContainsKey(CurrencyType.RebuildToken))
                    dbPlayer.Currencies[CurrencyType.RebuildToken] = 0;

                dbPlayer.Currencies[CurrencyType.RebuildToken]++;

                DB.Set(dbPlayer);
            }
        }

        protected void RefundPerksByMapping(Dictionary<(PerkType, int), int> refundMap)
        {
            var dbPlayers = DB.Players.ToList();
            foreach (var dbPlayer in dbPlayers)
            {
                var refundAmount = 0;

                // Calculate the refund amount first.
                foreach (var ((type, level), sp) in refundMap)
                {
                    if (dbPlayer.Perks.ContainsKey(type) && dbPlayer.Perks[type] >= level)
                    {
                        refundAmount += sp;
                    }
                }

                // Then remove the perks being refunded.
                foreach (var ((type, _), _) in refundMap)
                {
                    if (dbPlayer.Perks.ContainsKey(type))
                    {
                        dbPlayer.Perks.Remove(type);
                    }
                }

                if (refundAmount > 0)
                {
                    dbPlayer.UnallocatedSP += refundAmount;

                    Log.Write(LogGroup.Migration, $"{dbPlayer.Name} ({dbPlayer.Id}) refunded {refundAmount} SP.");

                    DB.Set(dbPlayer);
                }
            }
        }
    }
}
