using Vortex.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.IRoomObjectCreator
public interface IRoomObjectCreator
{
	bool AddObjectFurniture(int roomId, int objectId, int typeId, IVector3d location, IVector3d direction,
		int state, IStuffData stuffData, double extra = double.NaN, int expiryTime = -1,
		int usagePolicy = 0, int ownerId = 0, string ownerName = "",
		bool synchronized = true, bool realRoomObject = true, double sizeZ = -1);

	bool AddObjectFurnitureByName(int roomId, int objectId, string type, IVector3d location,
		IVector3d direction, int state, IStuffData stuffData, double extra = double.NaN);

	bool UpdateObjectFurniture(int roomId, int objectId, IVector3d location, IVector3d direction,
		int state, IStuffData stuffData, double extra = double.NaN);

	bool UpdateObjectFurnitureHeight(int roomId, int objectId, double height);

	bool UpdateObjectFurnitureLocation(int roomId, int objectId, IVector3d location,
		IVector3d direction, IVector3d? targetLocation = null, double extra = double.NaN);

	bool UpdateObjectFurnitureExpiryTime(int roomId, int objectId, int expiryTime);

	void DisposeObjectFurniture(int roomId, int objectId, int delay = -1, bool expired = false);

	bool AddObjectWallItem(int roomId, int objectId, int typeId, IVector3d location,
		IVector3d direction, int state, string data, int usagePolicy = 0, int ownerId = 0,
		string ownerName = "", int expiryTime = -1, bool realRoomObject = true);

	bool UpdateObjectWallItem(int roomId, int objectId, IVector3d location, IVector3d direction,
		int state, string data);

	bool UpdateObjectWallItemState(int roomId, int objectId, int state, string data);

	bool UpdateObjectWallItemData(int roomId, int objectId, string data);

	bool UpdateObjectWallItemLocation(int roomId, int objectId, IVector3d location,
		IVector3d? direction = null, double extra = double.NaN);

	bool UpdateObjectWallItemExpiryTime(int roomId, int objectId, int expiryTime);

	void DisposeObjectWallItem(int roomId, int objectId, int delay = -1);

	bool AddObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
		double headDirection, int userType, string? figure = null);

	bool UpdateObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
		bool canStandUp = false, double baseY = 0, IVector3d? targetLocation = null,
		double headDirection = double.NaN, double countdownTime = double.NaN,
		bool isSlide = false);

	bool UpdateObjectUserDir(int roomId, int objectId, IVector3d direction, double headDirection);

	bool UpdateObjectUserFlatControl(int roomId, int objectId, string level);

	bool UpdateObjectUserOwnUserAvatar(int roomId, int objectId);

	bool UpdateObjectUserFigure(int roomId, int objectId, string figure, string? gender = null,
		string? club = null, bool isRiding = false);

	bool UpdateObjectUserAction(int roomId, int objectId, string action, int value,
		string? parameter = null);

	bool UpdateObjectUserPosture(int roomId, int objectId, string posture, string parameter = "");

	bool UpdateObjectUserGesture(int roomId, int objectId, int gesture);

	bool UpdateObjectPetGesture(int roomId, int objectId, string gesture);

	bool UpdateObjectUserEffect(int roomId, int objectId, int effectId, int delay = 0);

	void DisposeObjectUser(int roomId, int objectId);

	bool UpdateObjectRoom(int roomId, string? floorType = null, string? wallType = null,
		string? landscapeType = null, bool animate = false);

	bool UpdateObjectRoomColor(int roomId, uint color, int brightness, bool bgOnly);

	bool UpdateObjectRoomBackgroundColor(int roomId, bool enable, int hue, int saturation, int lightness);

	bool UpdateObjectRoomVisibilities(int roomId, bool wallsVisible, bool floorVisible = true);

	bool UpdateObjectRoomPlaneThicknesses(int roomId, double wallThickness, double floorThickness);

	bool UpdateAreaHide(int roomId, int objectId, bool isOn, int rootX, int rootY,
		int width, int height, bool invert);

	void SetRoomObjectAlias(string alias, string target);

	int GetPetTypeId(string type);
}
