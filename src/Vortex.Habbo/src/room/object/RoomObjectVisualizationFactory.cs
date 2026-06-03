using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Runtime;
using Vortex.Habbo.Avatar;
using Vortex.Habbo.Room.Object.Visualization.Avatar;
using Vortex.Habbo.Room.Object.Visualization.Furniture;
using Vortex.Habbo.Room.Object.Visualization.Pet;
using Vortex.Habbo.Room.Object.Visualization.Room;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object;

/// @see com.sulake.habbo.room.object.RoomObjectVisualizationFactory
/// @see WIN63-202407091256-704579380-Source-main/habbo/room/object/RoomObjectVisualizationFactory.as
/// @see WIN63-202111081545-75921380-Source-main/habbo/room/object/RoomObjectVisualizationFactory.as (deobfuscation)
public class RoomObjectVisualizationFactory : Component, IRoomObjectVisualizationFactory
{
    private IAvatarRenderManager? _habboAvatar;
    private Dictionary<string, IRoomObjectVisualizationData>? _visualizationDataCache;
    private readonly bool _cacheEnabled;

    /// @see RoomObjectVisualizationFactory.as::RoomObjectVisualizationFactory
    public RoomObjectVisualizationFactory(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        _cacheEnabled = param2 == 0;
        _visualizationDataCache = new Dictionary<string, IRoomObjectVisualizationData>(StringComparer.Ordinal);
    }

    /// @see RoomObjectVisualizationFactory.as::dependencies
    protected override IList<ComponentDependency> dependencies
    {
        get
        {
            List<ComponentDependency> deps =
            [
                .. base.dependencies,
                new ComponentDependency(
                    new IID.IIDAvatarRenderManager(),
                    o => _habboAvatar = o as IAvatarRenderManager,
                    false
                ),
            ];
            return deps;
        }
    }

    /// @see RoomObjectVisualizationFactory.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_visualizationDataCache != null)
        {
            foreach (IRoomObjectVisualizationData data in _visualizationDataCache.Values)
            {
                data.Dispose();
            }

            _visualizationDataCache.Clear();
            _visualizationDataCache = null;
        }

        base.Dispose();
    }

    /// @see RoomObjectVisualizationFactory.as::createRoomObjectVisualization
    public IRoomObjectGraphicVisualization? CreateRoomObjectVisualization(string type)
    {
        IRoomObjectGraphicVisualization? visualization = type switch
        {
            "room" => new RoomVisualization(),
            "tile_cursor" => new TileCursorVisualization(),
            "user" or "bot" or "rentable_bot" => new AvatarVisualization(),
            "pet_animated" => new AnimatedPetVisualization(),
            "furniture_static" => new FurnitureVisualization(),
            "furniture_animated" => new AnimatedFurnitureVisualization(),
            "furniture_resetting_animated" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use ResettingAnimatedFurnitureVisualization once ported
            "furniture_poster" => new FurniturePosterVisualization(),
            "furniture_habbowheel" => new FurnitureHabboWheelVisualization(),
            "furniture_val_randomizer" => new FurnitureValRandomizerVisualization(),
            "furniture_bottle" => new FurnitureBottleVisualization(),
            "furniture_planet_system" => new FurniturePlanetSystemVisualization(),
            "furniture_queue_tile" => new FurnitureQueueTileVisualization(),
            "furniture_party_beamer" => new FurniturePartyBeamerVisualization(),
            "furniture_cuboid" => new FurnitureCuboidVisualization(),
            "furniture_gift_wrapped" => new FurnitureGiftWrappedVisualization(),
            "furniture_counter_clock" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureCounterClockVisualization once ported
            "furniture_water_area" => new FurnitureWaterAreaVisualization(),
            "furniture_score_board" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureScoreBoardVisualization once ported
            "furniture_fireworks" => new FurnitureFireworksVisualization(),
            "furniture_gift_wrapped_fireworks" => new FurnitureGiftWrappedFireworksVisualization(),
            "furniture_bb" => new FurnitureRoomBillboardVisualization(),
            "furniture_bg" => new FurnitureRoomBackgroundVisualization(),
            "furniture_stickie" => new FurnitureStickieVisualization(),
            "furniture_mannequin" => new FurnitureMannequinVisualization(),
            "furniture_guild_customized" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureGuildCustomizedVisualization once ported
            "furniture_guild_isometric_badge" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureGuildIsometricBadgeVisualization once ported
            "game_snowball" => new AnimatedFurnitureVisualization(), // TODO(as3-port): Use SnowballVisualization once ported
            "game_snowsplash" => new AnimatedFurnitureVisualization(), // TODO(as3-port): Use SnowSplashVisualization once ported
            "furniture_vote_counter" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureVoteCounterVisualization once ported
            "furniture_vote_majority" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureVoteMajorityVisualization once ported
            "furniture_soundblock" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureSoundBlockVisualization once ported
            "furniture_badge_display" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureBadgeDisplayVisualization once ported
            "furniture_youtube" =>
                new AnimatedFurnitureVisualization(), // TODO(as3-port): Use FurnitureYoutubeDisplayVisualization once ported
            "furniture_external_image" => new FurnitureExternalImageVisualization(),
            "furniture_builder_placeholder" => new FurnitureBuilderPlaceholderVisualization(),
            _ => null,
        };

        return visualization;
    }

    /// @see RoomObjectVisualizationFactory.as::getRoomObjectVisualizationData
    public IRoomObjectVisualizationData? GetRoomObjectVisualizationData(string type, string visualization, XElement? xml)
    {
        if (_visualizationDataCache != null &&
            _visualizationDataCache.TryGetValue(type, out IRoomObjectVisualizationData? cached))
        {
            return cached;
        }

        IRoomObjectVisualizationData? data = CreateVisualizationData(visualization);

        if (data == null)
        {
            return null;
        }

        if (!data.Initialize(xml))
        {
            data.Dispose();
            return null;
        }

        if (data is AvatarVisualizationData avatarData)
        {
            avatarData.AvatarRenderer = _habboAvatar;
        }
        else if (data is AnimatedPetVisualizationData petData)
        {
            petData.CommonAssets = assets as IAssetLibrary;
        }
        else if (data is AvatarFurnitureVisualizationData mannequinData)
        {
            mannequinData.AvatarRenderer = _habboAvatar;
        }
        // TODO(as3-port): Add SnowballVisualizationData branch once ported:
        // else if (data is SnowballVisualizationData snowData)
        // {
        //     snowData.Assets = assets;
        // }

        if (_cacheEnabled && _visualizationDataCache != null)
        {
            _visualizationDataCache[type] = data;
        }

        return data;
    }

    /// @see RoomObjectVisualizationFactory.as::createGraphicAssetCollection
    public IGraphicAssetCollection? CreateGraphicAssetCollection()
    {
        return new GraphicAssetCollection();
    }

    /// Creates a visualization data instance for the given visualization type string.
    /// @see RoomObjectVisualizationFactory.as::getRoomObjectVisualizationData (switch block)
    private static IRoomObjectVisualizationData? CreateVisualizationData(string visualization)
    {
        return visualization switch
        {
            "furniture_static"
                or "furniture_gift_wrapped"
                or "furniture_bb"
                or "furniture_bg"
                or "furniture_stickie"
                or "furniture_builder_placeholder"
                => new FurnitureVisualizationData(),

            "furniture_animated"
                or "furniture_resetting_animated"
                or "furniture_poster"
                or "furniture_habbowheel"
                or "furniture_val_randomizer"
                or "furniture_bottle"
                or "furniture_planet_system"
                or "furniture_queue_tile"
                or "furniture_party_beamer"
                or "furniture_counter_clock"
                or "furniture_water_area"
                or "furniture_score_board"
                or "furniture_fireworks"
                or "furniture_gift_wrapped_fireworks"
                or "furniture_guild_customized"
                or "furniture_guild_isometric_badge"
                or "furniture_vote_counter"
                or "furniture_vote_majority"
                or "furniture_soundblock"
                or "furniture_badge_display"
                or "furniture_external_image"
                or "furniture_youtube"
                or "tile_cursor"
                => new AnimatedFurnitureVisualizationData(),

            "furniture_mannequin"
                => new AvatarFurnitureVisualizationData(),

            "room"
                => new RoomVisualizationData(),

            "user" or "bot" or "rentable_bot"
                => new AvatarVisualizationData(),

            "pet_animated"
                => new AnimatedPetVisualizationData(),

            // TODO(as3-port): Use SnowballVisualizationData once ported
            "game_snowball" or "game_snowsplash"
                => new AnimatedFurnitureVisualizationData(),

            _ => null,
        };
    }
}
