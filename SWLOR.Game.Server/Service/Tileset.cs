using System;
using System.Collections.Generic;
using System.Linq;
using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Core.NWNX;
using SWLOR.Game.Server.Core.NWNX.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Service.TilesetService;
using TilesetData = SWLOR.Game.Server.Core.NWNX.Enum.TilesetData;

namespace SWLOR.Game.Server.Service
{
    public static class Tileset
    {

        private static readonly Dictionary<string, List<TileGroup>> _tileGroupByResref = new();
        private static readonly Dictionary<string, List<TileDetail>> _tilesByTileset = new();
        private static readonly Dictionary<string, TilesetData> _tilesetDataByResref = new();
        private static readonly Dictionary<string, TilesetDetail> _tilesetDetailByResref = new();
        private static readonly Dictionary<string, Dictionary<int, List<TileDoor>>> _tileDoors = new();

        private static readonly Dictionary<string, List<string>> _ignoredTerrainOrCrossers = new();

        [NWNEventHandler("mod_cache")]
        public static void ProcessTilesets()
        {
            ProcessTileset(TilesetResref.BeholderCaves);
            ProcessTileset(TilesetResref.CastleInterior);
            ProcessTileset(TilesetResref.CityExterior);
            ProcessTileset(TilesetResref.CityInterior);
            ProcessTileset(TilesetResref.Crypt);
            ProcessTileset(TilesetResref.Desert);
            ProcessTileset(TilesetResref.DrowInterior);
            ProcessTileset(TilesetResref.Dungeon);
            ProcessTileset(TilesetResref.Forest);
            ProcessTileset(TilesetResref.FrozenWastes);
            ProcessTileset(TilesetResref.IllithidInterior);
            ProcessTileset(TilesetResref.Microset);
            ProcessTileset(TilesetResref.MinesAndCaverns);
            ProcessTileset(TilesetResref.Ruins);
            ProcessTileset(TilesetResref.Rural);
            ProcessTileset(TilesetResref.RuralWinter);
            ProcessTileset(TilesetResref.Sewers);
            ProcessTileset(TilesetResref.Underdark);
            ProcessTileset(TilesetResref.LizardfolkInterior);
            ProcessTileset(TilesetResref.MedievalCity2);
            ProcessTileset(TilesetResref.MedievalRural2);
            ProcessTileset(TilesetResref.EarlyWinter2);
            ProcessTileset(TilesetResref.Seaships);
            ProcessTileset(TilesetResref.ForestFacelift);
            ProcessTileset(TilesetResref.RuralWinterFacelift);
            ProcessTileset(TilesetResref.Steamworks);
            ProcessTileset(TilesetResref.BarrowsInterior);
            ProcessTileset(TilesetResref.SeaCaves);
            ProcessTileset(TilesetResref.CityInterior2);
            ProcessTileset(TilesetResref.CastleInterior2);
            ProcessTileset(TilesetResref.CastleExteriorRural);
            ProcessTileset(TilesetResref.Tropical);
            ProcessTileset(TilesetResref.FortInterior);

            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2] = new List<string>();
            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2].Add("Chasm");
            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2].Add("Road");
            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2].Add("Wall");
            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2].Add("Bridge");
            _ignoredTerrainOrCrossers[TilesetResref.MedievalRural2].Add("Street");
        }

        private static string GetTerrainAndCrossersAsString(TileEdgesAndCorners str)
        {
            return str.TopLeft + 
                   str.Top + 
                   str.TopRight + 
                   str.Right + 
                   str.BottomRight + 
                   str.Bottom + 
                   str.BottomLeft +
                   str.Left;
        }

        private static void ProcessTileset(string tileset)
        {
            var tilesetData = TilesetPlugin.GetTilesetData(tileset);
            _tilesetDataByResref[tileset] = tilesetData;

            var name = (tilesetData.DisplayNameStringRef > 0
                ? GetStringByStrRef(tilesetData.DisplayNameStringRef)
                : tilesetData.UnlocalizedName);

            _tilesetDetailByResref[tileset] = new TilesetDetail
            {
                Name = name
            };

            for (var index = 0; index < tilesetData.TerrainCount; index++)
            {
                var terrain = CapitalizeString(TilesetPlugin.GetTilesetTerrain(tileset, index));
                _tilesetDetailByResref[tileset].Terrains.Add(terrain);
            }

            for (var index = 0; index < tilesetData.CrosserCount; index++)
            {
                var crosser = CapitalizeString(TilesetPlugin.GetTilesetCrosser(tileset, index));
                _tilesetDetailByResref[tileset].Crossers.Add(crosser);
            }

            ProcessGroups(tileset);

            for (var tileId = 0; tileId < tilesetData.TileCount; tileId++)
            {
                CheckForDoors(tileset, tileId);
                ProcessTile(tileset, tileId);
            }

            Console.WriteLine($"Processed tileset: {name}");
        }

        private static string CapitalizeString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        private static void InsertTile(
            string tileset, 
            int tileId, 
            int orientation, 
            int height, 
            TileEdgesAndCorners str)
        {
            if (!_tilesByTileset.ContainsKey(tileset))
                _tilesByTileset[tileset] = new List<TileDetail>();

            var tile = new TileDetail
            {
                TileId = tileId,
                Orientation = orientation,
                Height = height,

                TopLeft = str.TopLeft,
                Top = str.Top,
                TopRight = str.TopRight,
                Right = str.Right,
                BottomRight = str.BottomRight,
                Bottom = str.Bottom,
                BottomLeft = str.BottomLeft,
                Left = str.Left,

                TerrainAndCrossers = GetTerrainAndCrossersAsString(str)
            };

            _tilesByTileset[tileset].Add(tile);
        }

        private static TileEdgesAndCorners RotateTileEdgesAndCorners(TileEdgesAndCorners str)
        {
            str.TopLeft = str.TopRight;
            str.Top = str.Right;
            str.TopRight = str.BottomRight;
            str.Right = str.Bottom;
            str.BottomRight = str.BottomLeft;
            str.Bottom = str.Left;
            str.BottomLeft = str.TopLeft;
            str.Left = str.Top;

            return str;
        }

        private static string HandleEdgeCase(string tileset, string edge, string corner1, string corner2)
        {
            if (!string.IsNullOrWhiteSpace(edge))
                return edge;
            else if (corner1 == corner2)
                edge = corner1;
            else
                edge = "N/A";

            return edge;
        }

        private static void FixCapitalization(TileEdgesAndCorners str)
        {
            str.TopLeft = CapitalizeString(str.TopLeft);
            str.Top = CapitalizeString(str.Top);
            str.TopRight = CapitalizeString(str.TopRight);
            str.Right = CapitalizeString(str.Right);
            str.BottomRight = CapitalizeString(str.BottomRight);
            str.Bottom = CapitalizeString(str.Bottom);
            str.BottomLeft = CapitalizeString(str.BottomLeft);
            str.Left = CapitalizeString(str.Left);
        }

        private static TileEdgesAndCorners GetTileEdgesAndCorners(string tileset, int tileId)
        {
            var str = TilesetPlugin.GetTileEdgesAndCorners(tileset, tileId);
            FixCapitalization(str);

            str.Top = HandleEdgeCase(tileset, str.Top, str.TopLeft, str.TopRight);
            str.Right = HandleEdgeCase(tileset, str.Right, str.TopRight, str.BottomRight);
            str.Bottom = HandleEdgeCase(tileset, str.Bottom, str.BottomRight, str.BottomLeft);
            str.Left = HandleEdgeCase(tileset, str.Left, str.BottomLeft, str.TopLeft);

            return str;
        }

        private static TileEdgesAndCorners GetCornersAndEdgesByOrientation(string tileset, int tileId, int orientation)
        {
            var str = GetTileEdgesAndCorners(tileset, tileId);

            if (orientation == 0)
            {
                return str;
            }

            for (var count = 0; count < orientation; count++)
            {
                str = RotateTileEdgesAndCorners(str);
            }

            return str;
        }

        private static bool HasTerrainOrCrosser(TileEdgesAndCorners str, string type)
        {
            return str.TopLeft == type ||
                   str.Top == type ||
                   str.TopRight == type ||
                   str.Right == type ||
                   str.BottomRight == type ||
                   str.Bottom == type ||
                   str.BottomLeft == type ||
                   str.Left == type;
        }

        private static TileEdgesAndCorners ReplaceTerrainOrCrosser(TileEdgesAndCorners str, string old, string @new)
        {
            if (str.TopLeft == old) str.TopLeft = @new;
            if (str.Top == old) str.Top = @new;
            if (str.TopRight == old) str.TopRight = @new;
            if (str.Right == old) str.Right = @new;
            if (str.BottomRight == old) str.BottomRight = @new;
            if (str.Bottom == old) str.Bottom = @new;
            if (str.BottomLeft == old) str.BottomLeft = @new;
            if (str.Left == old) str.Left = @new;

            return str;
        }

        private static bool IsGroupTile(string tileset, int tileId)
        {
            if (!_tilesByTileset.ContainsKey(tileset))
                return false;

            return _tilesByTileset[tileset].Exists(x => x.TileId == tileId);
        }

        private static bool IsWhitelistedGroupTile(string tileset, int tileId)
        {
            if (tileset == TilesetResref.Rural)
            {
                switch (tileId)
                {
                    case 113: // Shrine01
                    case 118: // Crystal
                    case 129: // Tree Hollow
                    case 130: // Menhir
                    case 131: // Anthill
                    case 205: // Ramp
                    case 241: // Tree
                    case 244: // Cave
                        return true;
                }
            }
            else if (tileset == TilesetResref.CityExterior)
            {
                switch (tileId)
                {
                    case 33: // GC_MainDoor
                    case 34: // GC_Breach
                    case 42: // Wall Gate
                    case 45: // Market01
                    case 46: // Market02
                    case 47: // Tree
                    case 48: // Wagon
                    case 49: // StreetLight
                    case 50: // SlumHouse01
                    case 51: // SlumHouse02
                    case 54: // House
                    case 114: // SlumMarket01
                    case 115: // SlumMarket02
                    case 124: // Fountain
                    case 125: // VegGarden
                    case 128: // FlowerGarden
                    case 129: // Construction
                    case 130: // BurningBuilding
                    case 134: // SewerEntrance02
                    case 146: // Gazebo
                    case 147: // Well
                    case 181: // Footbridge
                    case 187: // DockDoor
                    case 193: // BridgeDoor
                    case 198: // BW_Temple
                    case 215: // Boathouse
                    case 231: // GC_SmallDoor
                    case 258: // WallGap01
                    case 259: // WallGap02
                    case 260: // WallChunk
                    case 273: // CaveEntrance
                    case 274: // Ramp
                    case 298: // Boat
                    case 299: // ElevationDoor01
                    case 304: // BW_Breach
                    case 316: // ElevationTower2
                    case 317: // ElevationTower1
                        return true;
                }
            }

            return false;
        }

        private static void ProcessTile(string tileset, int tileId)
        {
            if (IsGroupTile(tileset, tileId) && !IsWhitelistedGroupTile(tileset, tileId))
                return;

            var str = GetTileEdgesAndCorners(tileset, tileId);

            if (tileset == TilesetResref.CityExterior)
            {
                if (tileId == 306)
                    return;
            }
            else if (tileset == TilesetResref.MedievalRural2)
            {
                if (tileId == 812)
                    return;

                if (tileId == 433)
                {
                    str.TopLeft = "grass2";
                    str.Top = "ridge";
                    str.TopRight = "grass+";
                    str.Right = "ridge";
                    str.BottomRight = "grass2";
                    str.Bottom = "ridge";
                    str.BottomLeft = "grass+";
                    str.Left = "ridge";
                }
            }

            for (var orientation = 0; orientation < 4; orientation++)
            {
                InsertTile(tileset, tileId, orientation, 0, str);
                str = RotateTileEdgesAndCorners(str);
            }

            if (tileset == TilesetResref.Rural)
            {
                if ((HasTerrainOrCrosser(str, "Stream") &&
                     !HasTerrainOrCrosser(str, "Grass+") &&
                     !HasTerrainOrCrosser(str, "Trees") &&
                     !HasTerrainOrCrosser(str, "Water")) ||
                    (HasTerrainOrCrosser(str, "Road") &&
                     !HasTerrainOrCrosser(str, "Grass+") &&
                     !HasTerrainOrCrosser(str, "Trees") &&
                     !HasTerrainOrCrosser(str, "Water")) ||
                    tileId == 120)
                {
                    str = ReplaceTerrainOrCrosser(GetTileEdgesAndCorners(tileset, tileId), "Grass", "Grass+");

                    for (var orientation = 0; orientation < 4; orientation++)
                    {
                        InsertTile(tileset, tileId, orientation, 1, str);
                        str = RotateTileEdgesAndCorners(str);
                    }
                }
            }
        }

        private static void ProcessGroups(string tileset)
        {
            if (!_tilesetDataByResref.ContainsKey(tileset))
                return;

            var tilesetData = _tilesetDataByResref[tileset];

            if (tilesetData.GroupCount <= 0)
                return;

            if (!_tileGroupByResref.ContainsKey(tileset))
                _tileGroupByResref[tileset] = new List<TileGroup>();

            for (var groupNum = 0; groupNum < tilesetData.GroupCount; groupNum++)
            {
                var groupData = TilesetPlugin.GetTilesetGroupData(tileset, groupNum);
                var groupTileCount = groupData.Rows * groupData.Columns;

                var group = new TileGroup
                {
                    GroupId = groupNum,
                    Columns = groupData.Columns,
                    Rows = groupData.Rows,
                    Name = groupData.Name, 
                    StrRef = groupData.StringRef
                };

                for (var groupTileIndex = 0; groupTileIndex < groupTileCount; groupTileIndex++)
                {
                    var groupTileId = TilesetPlugin.GetTilesetGroupTile(tileset, groupNum, groupTileIndex);
                    group.Tiles.Add(new TileGroupTile
                    {
                        GroupId = groupNum,
                        TileId = groupTileId,
                        TileIndex = groupTileIndex
                    });
                }

                _tileGroupByResref[tileset].Add(group);
            }
        }

        private static void CheckForDoors(string tileset, int tileId)
        {
            var doorCount = TilesetPlugin.GetTileNumDoors(tileset, tileId);

            if (doorCount <= 0)
                return;

            for (var index = 0; index < doorCount; index++)
            {
                var doorData = TilesetPlugin.GetTileDoorData(tileset, tileId, index);

                if(doorData.Type == -1)
                    continue;

                doorData.X += 5f;
                doorData.Y += 5f;
                doorData.Orientation += 90f;

                var door = new TileDoor
                {
                    Location = Location(
                        OBJECT_INVALID,
                        Vector3(doorData.X, doorData.Y, doorData.Z),
                        doorData.Orientation),
                    Resref = Get2DAString("doortypes", "TemplateResRef", doorData.Type)
                };

                if (!_tileDoors.ContainsKey(tileset))
                    _tileDoors[tileset] = new Dictionary<int, List<TileDoor>>();

                if (!_tileDoors[tileset].ContainsKey(tileId))
                    _tileDoors[tileset][tileId] = new List<TileDoor>();

                _tileDoors[tileset][tileId].Add(door);
            }
        }

        public static TileDetail GetRandomMatchingTile(string tileset, TileEdgesAndCorners str)
        {
            var tilesetDetail = _tilesetDetailByResref[tileset];
            var tilesetData = _tilesetDataByResref[tileset];
            var tiles = _tilesByTileset[tileset];

            tiles = tiles
                .Where(x =>  (string.IsNullOrWhiteSpace(str.TopLeft) || x.TopLeft == str.TopLeft) &&
                            (string.IsNullOrWhiteSpace(str.Top) || x.Top == str.Top) &&
                            (string.IsNullOrWhiteSpace(str.TopRight) || x.TopRight == str.TopRight) &&
                            (string.IsNullOrWhiteSpace(str.Right) || x.Right == str.Right) &&
                            (string.IsNullOrWhiteSpace(str.BottomRight) || x.BottomRight == str.BottomRight) &&
                            (string.IsNullOrWhiteSpace(str.Bottom) || x.Bottom == str.Bottom) &&
                            (string.IsNullOrWhiteSpace(str.BottomLeft) || x.BottomLeft == str.BottomLeft) &&
                            (string.IsNullOrWhiteSpace(str.Left) || x.Left == str.Left))
                .ToList();

            foreach (var terrain in tilesetDetail.Terrains)
            {
                if (_ignoredTerrainOrCrossers.ContainsKey(tileset) &&
                    _ignoredTerrainOrCrossers[tileset].Contains(terrain))
                {
                    tiles = tiles.Where(x => !x.TerrainAndCrossers.Contains(terrain)).ToList();
                }
            }

            foreach (var crosser in tilesetDetail.Crossers)
            {
                if (_ignoredTerrainOrCrossers.ContainsKey(tileset) &&
                    _ignoredTerrainOrCrossers[tileset].Contains(crosser))
                {
                    tiles = tiles.Where(x => !x.TerrainAndCrossers.Contains(crosser)).ToList();
                }
            }

            if (tiles.Count <= 0)
            {
                return new TileDetail
                {
                    TileId = -1,
                    Orientation = -1,
                    Height = -1
                };
            }
            else
            {
                return tiles[Random.Next(tiles.Count)];
            }
        }
    }
}
