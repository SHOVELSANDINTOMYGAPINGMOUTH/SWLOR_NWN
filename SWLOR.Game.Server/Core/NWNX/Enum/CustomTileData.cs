using SWLOR.Game.Server.Core.NWScript.Enum;

namespace SWLOR.Game.Server.Core.NWNX.Enum
{
    public class CustomTileData
    {
        public int TileId { get; set; }
        public int Orientation { get; set; }
        public int Height { get; set; }

        public TileMainLightColor MainLightColor1 { get; set; }
        public TileMainLightColor MainLightColor2 { get; set; }
        public TileSourceLightColor SourceLightColor1 { get; set; }
        public TileSourceLightColor SourceLightColor2 { get; set; }

        public bool AnimationLoop1 { get; set; }
        public bool AnimationLoop2 { get; set; }
        public bool AnimationLoop3 { get; set; }
    }
}
