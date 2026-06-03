using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;
using Vortex.Room;
using Vortex.Room.Object;
using Vortex.Room.Renderer;

using Vortex.Habbo.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.IRoomEngineServices
public interface IRoomEngineServices : IRoomObjectCreator
{
	IConnection? Connection { get; }

	object? Events { get; }

	// TODO: IHabboGameManager GameEngine { get; } — unported

	bool ActiveRoomHasHanditemControlBlocked { get; }

	bool IsDecorateMode { get; }

	bool IsGameMode { get; }

	int PlayerUnderCursor { get; }

	int ActiveRoomId { get; }

	ICoreConfiguration? Configuration { get; }

	// TODO: IRoomSessionManager RoomSessionManager { get; } — unported
	// TODO: ISessionDataManager SessionDataManager { get; } — unported
	// TODO: IHabboToolbar Toolbar { get; } — unported
	// TODO: IHabboCatalog Catalog { get; } — unported
	// TODO: IHabboWindowManager WindowManager { get; } — unported
	// TODO: IRoomAreaSelectionManager AreaSelectionManager { get; } — unported

	IRoomInstance? GetRoom(int roomId);

	int GetRoomObjectCategory(string type);

	IRoomObject? GetRoomObject(int roomId, int objectId, int category);

	IRoomObject? GetRoomObjectWithIndex(int roomId, int index, int category);

	int GetRoomObjectCount(int roomId, int category);

	void UpdateObjectRoomWindow(int roomId, int objectId, bool update = true);

	void SetObjectMoverIconSprite(int objectId, int category, bool isWallItem,
		string? type = null, IStuffData? stuffData = null, int colorIndex = -1,
		int typeId = -1, string? instanceData = null);

	void SetObjectMoverIconSpriteVisible(bool visible);

	void RemoveObjectMoverIconSprite();

	ISelectedRoomObjectData? GetSelectedObjectData(int roomId);

	void SetSelectedObjectData(int roomId, SelectedRoomObjectData? data);

	void SetPlacedObjectData(int roomId, SelectedRoomObjectData? data);

	ISelectedRoomObjectData? GetPlacedObjectData(int roomId);

	LegacyWallGeometry? GetLegacyGeometry(int roomId);

	FurniStackingHeightMap? GetFurniStackingHeightMap(int roomId);

	TileObjectMap? GetTileObjectMap(int roomId);

	IRoomObjectController? GetSelectionArrow(int roomId);

	IRoomObjectController? GetTileCursor(int roomId);

	bool GetIsPlayingGame(int roomId);

	bool GetActiveRoomIsPlayingGame();

	void RequestRoomAdImage(int roomId, int objectId, int category, string imageUrl, string type);

	void RequestMouseCursor(string type, int objectId, string cursor);

	void AddFloorHole(int roomId, int objectId);

	void RemoveFloorHole(int roomId, int objectId);

	IRoomRenderingCanvas? GetActiveRoomActiveCanvas();

	void RequestBadgeImageAsset(int roomId, int objectId, int category, string badgeId, bool isGroupBadge = true);

	bool IsAreaSelectionMode();

	bool IsMoveBlocked();

	bool IsWhereYouClickWhereYouGo();
}
