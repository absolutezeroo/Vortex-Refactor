using System.Xml.Linq;

using Vortex.Core.Runtime;
using Vortex.Room;
using Vortex.Room.Object;
using Vortex.Room.Utils;

using Vortex.Habbo.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.IRoomCreator
public interface IRoomCreator : IRoomObjectCreator
{
	ICoreConfiguration? Configuration { get; }

	// TODO: IRoomSessionManager, ISessionDataManager, IHabboWindowManager — unported

	void InitializeRoom(int roomId, XElement? xml);

	IRoomInstance? GetRoom(int roomId);

	void DisposeRoom(int roomId);

	void SetOwnUserId(int roomId, int userId);

	void SetWorldType(int roomId, string? type);

	IRoomObjectController? GetObjectRoom(int roomId);

	void SetFurniStackingHeightMap(int roomId, FurniStackingHeightMap? heightMap);

	FurniStackingHeightMap? GetFurniStackingHeightMap(int roomId);

	LegacyWallGeometry? GetLegacyGeometry(int roomId);

	RoomGeometry? GetRoomGeometry(int roomId);

	TileObjectMap? GetTileObjectMap(int roomId);

	double GetRoomNumberValue(int roomId, string key);

	string? GetRoomStringValue(int roomId, string key);

	void SetIsPlayingGame(int roomId, bool isPlaying);

	void LeaveSpectate();

	void SetHanditemControlBlocked(int roomId, bool blocked);

	void RefreshTileObjectMap(int roomId, string category);
}
