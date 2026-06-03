using System.Xml.Linq;

using Godot;

using Vortex.Core.Runtime;
using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.IRoomEngine
public interface IRoomEngine : IUnknown
{
	object? Events { get; }

	bool IsInitialized { get; }

	int ActiveRoomId { get; }

	bool IsDecorateMode { get; }

	bool IsGameMode { get; set; }

	bool DisableUpdate { set; }

	int MouseEventsDisabledAboveY { get; set; }

	int MouseEventsDisabledLeftToX { get; set; }

	// TODO: IRoomAreaSelectionManager AreaSelectionManager { get; } — unported

	bool ActiveRoomHasHanditemControlBlocked { get; }

	Node2D? CreateRoomCanvas(int roomId, int canvasId, int width, int height, int scale);

	void SetRoomCanvasScale(int roomId, int canvasId, double scale, Vector2? point = null,
		Vector2? offset = null, bool skipUpdate = false, bool center = false, bool noAnimation = false);

	bool ModifyRoomCanvas(int roomId, int canvasId, int width, int height);

	List<IRoomObject>? GetObjectsByCategory(int category);

	void SetRoomCanvasMask(int roomId, int canvasId, bool flag);

	IRoomGeometry? GetRoomCanvasGeometry(int roomId, int canvasId = -1);

	Vector2? GetRoomCanvasScreenOffset(int roomId, int canvasId = -1);

	bool SetRoomCanvasScreenOffset(int roomId, int canvasId, Vector2 offset);

	double GetRoomCanvasScale(int roomId = -1000, int canvasId = -1);

	void HandleRoomCanvasMouseEvent(int roomId, int canvasId, int x, int y, string type,
		bool altKey, bool ctrlKey, bool shiftKey, bool buttonDown);

	void SetActiveRoom(int roomId);

	double GetRoomNumberValue(int roomId, string key);

	string? GetRoomStringValue(int roomId, string key);

	string? GetFurnitureIconUrl(int typeId);

	ImageResult? GetFurnitureIcon(int typeId, IGetImageListener? listener, string? extra = null,
		IStuffData? stuffData = null, bool isIcon = false);

	string? GetWallItemIconUrl(int typeId, string? extra = null);

	ImageResult? GetWallItemIcon(int typeId, IGetImageListener? listener, string? extra = null);

	ImageResult? GetFurnitureImage(int typeId, IVector3d direction, int scale,
		IGetImageListener? listener, uint bgColor = 0, string? extra = null,
		int state = -1, int frameCount = -1, IStuffData? stuffData = null, bool isIcon = false);

	ImageResult? GetGenericRoomObjectImage(string type, string value, IVector3d direction,
		int scale, IGetImageListener? listener, uint bgColor = 0, string? extra = null,
		IStuffData? stuffData = null, int state = -1, int frameCount = -1,
		string? posture = null, int entityId = -1);

	ImageResult? GetWallItemImage(int typeId, IVector3d direction, int scale,
		IGetImageListener? listener, uint bgColor = 0, string? extra = null,
		int state = -1, int frameCount = -1);

	ImageResult? GetPetImage(int typeId, int paletteId, int customPartCount,
		IVector3d direction, int scale, IGetImageListener? listener,
		bool headOnly = true, uint bgColor = 0, string[]? customParts = null,
		string? posture = null);

	ImageResult? GetRoomImage(string floorType, string wallType, string landscapeType,
		int scale, IGetImageListener? listener, string? xml = null);

	ImageResult? GetRoomObjectImage(int roomId, int objectId, int category,
		IVector3d direction, int scale, IGetImageListener? listener, uint bgColor = 0);

	Rect2? GetRoomObjectBoundingRectangle(int roomId, int objectId, int category, int canvasId);

	Vector2? GetRoomObjectScreenLocation(int roomId, int objectId, int category, int canvasId = -1);

	Rect2? GetActiveRoomBoundingRectangle(int canvasId);

	int GetRoomObjectCount(int roomId, int category);

	IRoomObject? GetRoomObject(int roomId, int objectId, int category);

	IRoomObject? GetRoomObjectWithIndex(int roomId, int index, int category);

	bool ModifyRoomObject(int objectId, int category, string operation);

	bool ModifyRoomObjectDataWithMap(int objectId, int category, string operation, Dictionary<string, string> data);

	bool ModifyRoomObjectData(int objectId, int category, string operation, string data);

	bool DeleteRoomObject(int objectId, int category);

	bool InitializeRoomObjectInsert(string type, int typeId, int category, int stuffDataKey,
		string? extra = null, IStuffData? stuffData = null, int state = -1, int frameCount = -1,
		string? instanceData = null, bool isReplace = false);

	void CancelRoomObjectInsert();

	void SelectAvatar(int roomId, int objectId);

	void SelectRoomObject(int roomId, int objectId, int category);

	string? GetWorldType(int roomId);

	ISelectedRoomObjectData? GetSelectedObjectData(int roomId);

	int GetSelectedAvatarId();

	bool UpdateObjectRoomColor(int roomId, uint color, int brightness, bool bgOnly);

	bool UpdateObjectRoomBackgroundColor(int roomId, bool enable, int hue, int saturation, int lightness);

	PetColorResult? GetPetColor(int typeId, int paletteId);

	string[]? GetPetColorsByTag(int typeId, string tag);

	int GetPetLayerIdForTag(int typeId, string tag);

	PetColorResult? GetPetDefaultPalette(int typeId, string tag);

	bool AddObjectFurniture(int roomId, int objectId, int typeId, IVector3d location,
		IVector3d direction, int state, IStuffData stuffData, double extra = double.NaN,
		int expiryTime = -1, int usagePolicy = 0, int ownerId = 0, string ownerName = "",
		bool synchronized = true, bool realRoomObject = true, double sizeZ = -1);

	void ChangeObjectState(int roomId, int objectId, int category);

	bool ChangeObjectModelData(int roomId, int objectId, int category, string key, int value);

	void DisposeObjectFurniture(int roomId, int objectId, int delay = -1, bool expired = false);

	bool AddObjectWallItem(int roomId, int objectId, int typeId, IVector3d location,
		IVector3d direction, int state, string data, int usagePolicy = 0, int ownerId = 0,
		string ownerName = "", int expiryTime = -1, bool realRoomObject = true);

	bool UpdateObjectWallItemLocation(int roomId, int objectId, IVector3d location,
		IVector3d? direction = null, double extra = double.NaN);

	void DisposeObjectWallItem(int roomId, int objectId, int delay = -1);

	bool AddObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
		double headDirection, int userType, string? figure = null);

	bool AddObjectSnowWar(int roomId, int objectId, IVector3d location, int frame);

	bool UpdateObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
		bool canStandUp = false, double baseY = 0, IVector3d? targetLocation = null,
		double headDirection = double.NaN, double countdownTime = double.NaN,
		bool isSlide = false);

	bool UpdateObjectUserFigure(int roomId, int objectId, string figure, string? gender = null,
		string? club = null, bool isRiding = false);

	bool UpdateObjectUserPosture(int roomId, int objectId, string posture, string parameter = "");

	bool UpdateObjectUserGesture(int roomId, int objectId, int gesture);

	bool UpdateObjectUserEffect(int roomId, int objectId, int effectId, int delay = 0);

	bool UpdateObjectSnowWar(int roomId, int objectId, IVector3d location, int frame);

	void DisposeObjectSnowWar(int roomId, int objectId, int delay);

	bool UpdateObjectUserAction(int roomId, int objectId, string action, int value,
		string? parameter = null);

	void DisposeObjectUser(int roomId, int objectId);

	bool UpdateObjectRoom(int roomId, string? floorType = null, string? wallType = null,
		string? landscapeType = null, bool animate = false);

	string? GetFurnitureType(int typeId);

	int GetFurnitureTypeId(string type);

	string? GetWallItemType(int typeId, string? extra = null);

	bool UseRoomObjectInActiveRoom(int objectId, int category);

	void InitializeRoom(int roomId, XElement? xml);

	void DisposeRoom(int roomId);

	void ShowUseProductSelection(int objectId, int category, int targetCategory = -1);

	void SetAvatarEffect(int effectId);

	void SetTileCursorState(int roomId, int state);

	void ToggleTileCursorVisibility(int roomId, bool visible);

	void AddObjectUpdateCategory(int category);

	void RemoveObjectUpdateCategory(int category);

	bool SnapshotRoomCanvasToBitmap(int roomId, int canvasId, Image data, Transform2D transform, bool clip);

	void RunUpdate();

	bool UpdateObjectRoomVisibilities(int roomId, bool wallsVisible, bool floorVisible = true);

	// TODO: IMessageComposer GetRenderRoomMessage(...) — unported composer

	void CreateScreenShot(int roomId, int canvasId, string format);

	void PurgeRoomContent();

	void SetMoveBlocked(bool blocked);
}
