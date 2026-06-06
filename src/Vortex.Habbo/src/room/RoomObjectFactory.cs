using System;

using Vortex.Core.Runtime;
using Vortex.Habbo.Room.Object.Logic;
using Vortex.Room;
using Vortex.Room.Object.Logic;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.RoomObjectFactory
/// @see WIN63-202407091256-704579380-Source-main/habbo/room/RoomObjectFactory.as
/// @see WIN63-202111081545-75921380-Source-main/habbo/room/RoomObjectFactory.as (deobfuscation)
public class RoomObjectFactory : Component, IRoomObjectFactory
{
    private readonly Dictionary<string, bool> _registeredLogicTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _trackedEventTypes = new(StringComparer.Ordinal);
    private readonly List<Action<object?>> _objectEventListeners = [];

    /// @see RoomObjectFactory.as::RoomObjectFactory
    public RoomObjectFactory(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
    }

    /// @see RoomObjectFactory.as::events
    public object? Events => events;

    /// @see RoomObjectFactory.as::addObjectEventListener
    public void AddObjectEventListener(Action<object?> listener)
    {
        if (_objectEventListeners.Contains(listener))
        {
            return;
        }

        _objectEventListeners.Add(listener);

        foreach (string eventType in _trackedEventTypes.Keys)
        {
            events.AddEventListener(eventType, listener);
        }
    }

    /// @see RoomObjectFactory.as::removeObjectEventListener
    public void RemoveObjectEventListener(Action<object?> listener)
    {
        int index = _objectEventListeners.IndexOf(listener);

        if (index < 0)
        {
            return;
        }

        _objectEventListeners.RemoveAt(index);

        foreach (string eventType in _trackedEventTypes.Keys)
        {
            events.RemoveEventListener(eventType, listener);
        }
    }

    /// @see RoomObjectFactory.as::createRoomObjectLogic
    public IRoomObjectEventHandler? CreateRoomObjectLogic(string type)
    {
        IRoomObjectEventHandler? handler = type switch
        {
            "furniture_basic" => new FurnitureLogic(),
            "furniture_multistate" => new FurnitureMultiStateLogic(),
            "furniture_multiheight" => new FurnitureMultiHeightLogic(),
            "furniture_placeholder" => new FurniturePlaceholderLogic(),
            "user" or "bot" or "rentable_bot" => new AvatarLogic(),
            "pet" => new PetLogic(),
            "furniture_randomstate" => new FurnitureRandomStateLogic(),
            "furniture_credit" => new FurnitureCreditLogic(),
            "furniture_stickie" => new FurnitureStickieLogic(),
            "furniture_external_image_wallitem" => new FurnitureExternalImageLogic(),
            "furniture_present" => new FurniturePresentLogic(),
            "furniture_trophy" => new FurnitureTrophyLogic(),
            "furniture_ecotron_box" => new FurnitureEcotronBoxLogic(),
            "furniture_dice" => new FurnitureDiceLogic(),
            "furniture_hockey_score" => new FurnitureHockeyScoreLogic(),
            "furniture_habbowheel" => new FurnitureHabboWheelLogic(),
            "furniture_one_way_door" => new FurnitureOneWayDoorLogic(),
            "furniture_planet_system" => new FurniturePlanetSystemLogic(),
            "furniture_window" => new FurnitureWindowLogic(),
            "furniture_roomdimmer" => new FurnitureRoomDimmerLogic(),
            "tile_cursor" => new RoomTileCursorLogic(),
            "selection_arrow" => new SelectionArrowLogic(),
            "furniture_sound_machine" => new FurnitureSoundMachineLogic(),
            "furniture_jukebox" => new FurnitureJukeboxLogic(),
            "furniture_crackable" => new FurnitureCrackableLogic(),
            "furniture_song_disk" => new FurnitureSongDiskLogic(),
            "furniture_pushable" => new FurniturePushableLogic(),
            "furniture_clothing_change" => new FurnitureClothingChangeLogic(),
            "furniture_counter_clock" => new FurnitureCounterClockLogic(),
            "furniture_score" => new FurnitureScoreLogic(),
            "furniture_es" => new FurnitureIceStormLogic(),
            "furniture_fireworks" => new FurnitureFireworksLogic(),
            "furniture_bb" => new FurnitureRoomBillboardLogic(),
            "furniture_bg" => new FurnitureRoomBackgroundLogic(),
            "furniture_welcome_gift" => new FurnitureWelcomeGiftLogic(),
            "furniture_floor_hole" => new FurnitureFloorHoleLogic(),
            "room" => new RoomLogic(),
            "furniture_mannequin" => new FurnitureMannequinLogic(),
            "furniture_guild_customized" => new FurnitureGuildCustomizedLogic(),
            "furniture_group_forum_terminal" => new FurnitureGroupForumTerminalLogic(),
            "furniture_pet_customization" => new FurniturePetProductLogic(),
            "game_snowball" => new SnowballLogic(),
            "game_snowsplash" => new SnowSplashLogic(),
            "furniture_cuckoo_clock" => new FurnitureCuckooClockLogic(),
            "furniture_vote_counter" => new FurnitureVoteCounterLogic(),
            "furniture_vote_majority" => new FurnitureVoteMajorityLogic(),
            "furniture_soundblock" => new FurnitureSoundBlockLogic(),
            "furniture_random_teleport" => new FurnitureRandomTeleportLogic(),
            "furniture_monsterplant_seed" => new FurnitureMonsterplantSeedLogic(),
            "furniture_purchasable_clothing" => new FurniturePurchasableClothingLogic(),
            "furniture_background_color" => new FurnitureRoomBackgroundColorLogic(),
            "furniture_area_hide" => new FurnitureMultiStateLogic(), // TODO(as3-port): Use FurnitureAreaHideLogic once ported
            "furniture_mysterybox" => new FurnitureMysterboxLogic(),
            "furniture_effectbox" => new FurnitureEffectboxLogic(),
            "furniture_mysterytrophy" => new FurnitureMysteryTrophyLogic(),
            "furniture_achievement_resolution" => new FurnitureAchievementResolutionLogic(),
            "furniture_lovelock" => new FurnitureLovelockLogic(),
            "furniture_wildwest_wanted" => new FurnitureMultiStateLogic(), // TODO(as3-port): Use FurnitureWildwestWantedLogic once ported
            "furniture_hween_lovelock" => new FurnitureHweenLovelockLogic(),
            "furniture_badge_display" => new FurnitureBadgeDisplayLogic(),
            "furniture_high_score" => new FurnitureHighScoreLogic(),
            "furniture_internal_link" => new FurnitureInternalLinkLogic(),
            "furniture_editable_internal_link" => new FurnitureEditableInternalLinkLogic(),
            "furniture_editable_room_link" => new FurnitureEditableRoomLinkLogic(),
            "furniture_custom_stack_height" => new FurnitureCustomStackHeightLogic(),
            "furniture_youtube" => new FurnitureYoutubeLogic(),
            "furniture_rentable_space" => new FurnitureRentableSpaceLogic(),
            "furniture_change_state_when_step_on" => new FurnitureChangeStateWhenStepOnLogic(),
            "furniture_vimeo" => new FurnitureVimeoLogic(),
            "furniture_crafting_gizmo" => new FurnitureCraftingGizmoLogic(),
            _ => null,
        };

        if (handler == null)
        {
            return null;
        }

        handler.EventDispatcher = events;

        if (!_registeredLogicTypes.TryAdd(type, true))
        {
            return handler;
        }

        string[]? eventTypes = handler.GetEventTypes();

        if (eventTypes == null)
        {
            return handler;
        }

        foreach (string eventType in eventTypes)
        {
            AddTrackedEventType(eventType);
        }

        return handler;
    }

    /// @see RoomObjectFactory.as::createRoomObjectManager
    public IRoomObjectManager CreateRoomObjectManager()
    {
        return new RoomObjectManager();
    }

    /// @see RoomObjectFactory.as::addTrackedEventType
    private void AddTrackedEventType(string eventType)
    {
        if (!_trackedEventTypes.TryAdd(eventType, true))
        {
            return;
        }

        foreach (Action<object?> listener in _objectEventListeners)
        {
            events.AddEventListener(eventType, listener);
        }
    }
}
