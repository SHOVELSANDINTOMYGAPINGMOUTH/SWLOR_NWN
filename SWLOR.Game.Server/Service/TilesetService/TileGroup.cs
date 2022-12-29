using System.Collections.Generic;

namespace SWLOR.Game.Server.Service.TilesetService
{
    public class TileGroup
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public int StrRef { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<TileGroupTile> Tiles { get; set; }

        public TileGroup()
        {
            Tiles = new List<TileGroupTile>();
        }
    }
}
