using System.Linq;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.DBService;

namespace SWLOR.Game.Server.Feature.MigrationDefinition.ServerMigration
{
    public class _4_ResaveNotes : ServerMigrationBase
    {
        public int Version => 4;
        public void Migrate()
        {
            var notes = DB.PlayerNotes.ToList();

            foreach (var note in notes)
            {
                note.DMCreatorCDKey = string.Empty;
                note.DMCreatorName = string.Empty;
                note.IsDMNote = false;

                DB.Set(note);
            }
        }
    }
}
