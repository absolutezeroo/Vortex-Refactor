using Godot;

using Vortex.Room.Data;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Renderer;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.SpriteDataCollector
public class SpriteDataCollector
{
	private const int MANNEQUIN_MAGIC_X_OFFSET = 1;
	private const int MANNEQUIN_MAGIC_Y_OFFSET = -16;
	private const int AVATAR_WATER_EFFECT_MAGIC_Y_OFFSET = -52;
	private const int MAX_EXTERNAL_IMAGE_COUNT = 30;

	private double _maxZ;
	private int _spriteCount;
	private int _externalImageCount;

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::sortSpriteDataObjects
	private static int SortSpriteDataObjects(RoomObjectSpriteData a, RoomObjectSpriteData b)
	{
		if (a.Z < b.Z)
		{
			return 1;
		}

		if (a.Z > b.Z)
		{
			return -1;
		}

		return -1;
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::isSpriteInViewPort
	private static bool IsSpriteInViewPort(RoomObjectSpriteData sprite, Rect2 viewport, IRoomRenderingCanvas canvas)
	{
		Rect2 spriteRect = new Rect2(
			sprite.X + canvas.ScreenOffsetX,
			sprite.Y + canvas.ScreenOffsetY,
			sprite.Width,
			sprite.Height);

		return spriteRect.Intersects(viewport);
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::sortQuadPoints
	private static List<Vector2> SortQuadPoints(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{
		List<Vector2> points;

		if (p1.X == p2.X)
		{
			points = [p1, p3, p2, p4];
		}
		else if (p1.X == p3.X)
		{
			points = [p1, p2, p3, p4];
		}
		else if ((p2.X < p1.X && p2.Y > p1.Y) || (p2.X > p1.X && p2.Y < p1.Y))
		{
			points = [p1, p3, p2, p4];
		}
		else
		{
			points = [p1, p2, p3, p4];
		}

		if (points[0].X < points[1].X)
		{
			(points[0], points[1]) = (points[1], points[0]);
			(points[2], points[3]) = (points[3], points[2]);
		}

		if (points[0].Y < points[2].Y)
		{
			(points[0], points[2]) = (points[2], points[0]);
			(points[1], points[3]) = (points[3], points[1]);
		}

		return points;
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::addMannequinSprites
	// TODO(room): Implement — replaces boutique_mannequin1 sprites with avatar sprite list
	private static List<RoomObjectSpriteData> AddMannequinSprites(
		List<RoomObjectSpriteData> sprites, RoomEngine roomEngine)
	{
		GD.PushWarning("[SpriteDataCollector] AddMannequinSprites not implemented");
		return sprites;
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::getFurniData
	// TODO(room): Implement — collects avatar/furniture sprite data, returns JSON string
	public string GetFurniData(
		Rect2 viewport, IRoomRenderingCanvas canvas, RoomEngine roomEngine, int excludeObjectId)
	{
		GD.PushWarning("[SpriteDataCollector] GetFurniData not implemented");
		return "[]";
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::getRoomRenderingModifiers
	// TODO(room): Implement — returns JSON of room rendering modifiers (currently empty in AS3)
	public string GetRoomRenderingModifiers(RoomEngine roomEngine)
	{
		return "{}";
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::getSpriteDataObject
	// TODO(room): Implement — converts RoomObjectSpriteData to serializable data object
	private object? GetSpriteDataObject(
		RoomObjectSpriteData sprite, Rect2 viewport, IRoomRenderingCanvas canvas, RoomEngine roomEngine)
	{
		GD.PushWarning("[SpriteDataCollector] GetSpriteDataObject not implemented");
		return null;
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::makeBackgroundPlane
	// TODO(room): Implement — creates background PlaneDrawingData covering the viewport
	// Blocked on: IPlaneDrawingData (not yet ported)
	private object? MakeBackgroundPlane(Rect2 viewport, uint color, List<object> existingPlanes)
	{
		GD.PushWarning("[SpriteDataCollector] MakeBackgroundPlane not implemented");
		return null;
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::sortRoomPlanes
	// TODO(room): Implement — sorts room planes by z-depth using canvas sortable sprites
	// Blocked on: IPlaneVisualization (not yet ported)
	private List<object> SortRoomPlanes(
		List<IRoomPlane> planes, IRoomRenderingCanvas canvas, RoomEngine roomEngine)
	{
		GD.PushWarning("[SpriteDataCollector] SortRoomPlanes not implemented");
		return [];
	}

	/// @see com.sulake.habbo.room.utils.SpriteDataCollector::getRoomPlanes
	// TODO(room): Implement — gets sorted room planes projected to screen coordinates
	// Blocked on: IPlaneVisualization, IPlaneDrawingData (not yet ported)
	public List<object> GetRoomPlanes(
		Rect2 viewport, IRoomRenderingCanvas canvas, RoomEngine roomEngine, uint backgroundColor)
	{
		GD.PushWarning("[SpriteDataCollector] GetRoomPlanes not implemented");
		return [];
	}
}
