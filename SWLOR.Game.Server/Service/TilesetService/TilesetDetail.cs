using System.Collections.Generic;

namespace SWLOR.Game.Server.Service.TilesetService
{
    public class TilesetDetail
    {
        public string Name { get; set; }
        public int NumberTileData { get; set; }
        public float HeightTransition { get; set; }
        public int NumberGroups { get; set; }
        public List<string> Terrains { get; set; }
        public List<string> Crossers { get; set; }
        public Dictionary<int, int> TileDoors { get; set; }


        public TilesetDetail()
        {
            Terrains = new List<string>();
            Crossers = new List<string>();
            TileDoors = new Dictionary<int, int>();
            
        }
    }
}
