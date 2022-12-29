using System.Collections.Generic;
using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Core.NWNX;
using SWLOR.Game.Server.Core.NWNX.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.ChatCommandService;

namespace SWLOR.Game.Server.Feature.ChatCommandDefinition
{
    public class DebuggingChatCommand: IChatCommandListDefinition
    {
        private readonly ChatCommandBuilder _builder = new();
        public Dictionary<string, ChatCommandDetail> BuildChatCommands()
        {
            //MoveDoor();
            Test();

            return _builder.Build();
        }

        private static Location GetDoorLocation(uint building, float orientationOverride = 0f, float sqrtValue = 0f)
        {
            var area = GetArea(building);
            var location = GetLocation(building);
            var orientationAdjustment = orientationOverride != 0f ? orientationOverride : 200.31f;
            var sqrtAdjustment = sqrtValue != 0f ? sqrtValue : 13.0f;

            var position = GetPositionFromLocation(location);
            var orientation = GetFacingFromLocation(location);

            orientation = orientation + orientationAdjustment;
            if (orientation > 360.0)
                orientation -= 360.0f;

            var mod = sqrt(sqrtAdjustment) * sin(orientation);
            position.X += mod;

            mod = sqrt(sqrtAdjustment) * cos(orientation);
            position.Y -= mod;
            var doorLocation = Location(area, position, orientation);
            return doorLocation;
        }

        private void MoveDoor()
        {
            _builder.Create("movedoor")
                .Description("Debugging")
                .Permissions(AuthorizationLevel.Admin)
                .Action((user, target, location, args) =>
                {
                    var orientationOverride = float.Parse(args[0]);
                    var sqrtValue = float.Parse(args[1]);
                    var placeable = GetObjectByTag("house1");

                    if (!GetIsObjectValid(placeable))
                    {
                        var waypoint = GetWaypointByTag("DEBUG_HOUSE");
                        placeable = CreateObject(ObjectType.Placeable, "house1", GetLocation(waypoint));
                    }

                    var doorLocation = GetDoorLocation(placeable, orientationOverride, sqrtValue);

                    var door = GetLocalObject(placeable, "PROPERTY_DOOR");
                    if (GetIsObjectValid(door))
                        DestroyObject(door);

                    door = CreateObject(ObjectType.Placeable, "building_ent1", doorLocation);
                    SetLocalObject(placeable, "PROPERTY_DOOR", door);

                    SendMessageToPC(user, $"{orientationOverride} {sqrtValue}");
                });
        }

        private void Test()
        {
            void SetTileData(string overrideName, int tileIndex, int tileId, int tileOrientation)
            {
                var customTile = new CustomTileData()
                {
                    TileId = tileId,
                    Orientation = tileOrientation,
                    Height = 0,
                    MainLightColor1 = TileMainLightColor.BrightWhite,
                    MainLightColor2 = TileMainLightColor.BrightWhite,
                    SourceLightColor1 = TileSourceLightColor.PaleYellow,
                    SourceLightColor2 = TileSourceLightColor.PaleYellow,
                    AnimationLoop1 = true,
                    AnimationLoop2 = true,
                    AnimationLoop3 = true
                };

                TilesetPlugin.SetOverrideTileData(overrideName, tileIndex, customTile);
            }

            _builder.Create("test")
                .Description("Debugging")
                .Permissions(AuthorizationLevel.Admin)
                .Action((user, target, location, args) =>
                {
                    const string OverrideName = "MyTileOverride";
                    var tileset = string.IsNullOrWhiteSpace(args[0]) 
                        ? TilesetResref.Microset 
                        : args[0];
                    const int Width = 32;
                    const int Height = 32;

                    TilesetPlugin.CreateTileOverride(OverrideName, tileset, Width, Height);

                    var tileCount = Width * Height;
                    for (var tileIndex = 0; tileIndex < tileCount; tileIndex++)
                    {
                        var str = TilesetPlugin.GetTileEdgesAndCorners(tileset, 0);
                        var tile = Tileset.GetRandomMatchingTile(tileset, str);

                        SetTileData(OverrideName, tileIndex, tile.TileId, tile.Orientation);
                    }

                    //SetTileData(OverrideName, 0, 35, 2);
                    //SetTileData(OverrideName, 1, 36, 3);
                    //SetTileData(OverrideName, 2, 35, 3);

                    //SetTileData(OverrideName, 3, 36, 2);
                    //SetTileData(OverrideName, 4, 120, 0);
                    //SetTileData(OverrideName, 5, 36, 0);

                    //SetTileData(OverrideName, 6, 35, 1);
                    //SetTileData(OverrideName, 7, 36, 1);
                    //SetTileData(OverrideName, 8, 35, 0);

                    TilesetPlugin.SetAreaTileOverride("area_template", OverrideName);
                    CreateArea("area_template", "MY_COOL_AREA", "Optional Cool Name");
                });
        }
    }
}
