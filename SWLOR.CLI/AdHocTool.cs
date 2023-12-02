using System;
using Redis.OM;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.DBService;

namespace SWLOR.CLI
{
    internal class AdHocTool
    {
        public void Process()
        {
            Environment.SetEnvironmentVariable("NWNX_REDIS_HOST", "localhost");

            DB.Load();
            
            var entities = DB.Players.Where(x => x.Name.Contains("Yasila"));
            foreach (var entity in entities)
            {
                Console.WriteLine($"{entity.Name} = {entity.Id}");
            }

        }
    }
}
