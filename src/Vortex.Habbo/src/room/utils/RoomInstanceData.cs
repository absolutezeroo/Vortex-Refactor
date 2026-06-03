namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.RoomInstanceData
public class RoomInstanceData(int roomId)
{
    private FurniStackingHeightMap? _furniStackingHeightMap;
    private SelectedRoomObjectData? _selectedObject;
    private SelectedRoomObjectData? _placedObject;
    private Dictionary<int, FurnitureData>? _furnitureDataMap = new();
    private Dictionary<int, FurnitureData>? _wallItemDataMap = new();
    private readonly List<string> _mouseButtonCursorOwners = [];

    public int RoomId { get; } = roomId;

    public bool IsDecorateMode { get; set; }

    public bool HanditemControlBlocked { get; set; }

    public bool IsPlayingGame { get; set; }

    public string? PendingFloorType { get; set; }

    public string? PendingWallType { get; set; }

    public string? PendingLandscapeType { get; set; }

    public FurniStackingHeightMap? FurniStackingHeightMap
    {
        get => _furniStackingHeightMap;
        set
        {
            _furniStackingHeightMap?.Dispose();
            _furniStackingHeightMap = value;

            TileObjectMap?.Dispose();

            if (_furniStackingHeightMap != null)
            {
                TileObjectMap = new TileObjectMap(_furniStackingHeightMap.Width, _furniStackingHeightMap.Height);
            }
        }
    }

    public LegacyWallGeometry? LegacyGeometry { get; private set; } = new();

    public TileObjectMap? TileObjectMap { get; private set; }

    public RoomCamera? RoomCamera { get; private set; } = new();

    public string? WorldType { get; set; }

    public SelectedRoomObjectData? SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject?.Dispose();
            _selectedObject = value;
        }
    }

    public SelectedRoomObjectData? PlacedObject
    {
        get => _placedObject;
        set
        {
            _placedObject?.Dispose();
            _placedObject = value;
        }
    }

    public void Dispose()
    {
        if (_furniStackingHeightMap != null)
        {
            _furniStackingHeightMap.Dispose();
            _furniStackingHeightMap = null;
        }

        if (LegacyGeometry != null)
        {
            LegacyGeometry.Dispose();
            LegacyGeometry = null;
        }

        if (RoomCamera != null)
        {
            RoomCamera.Dispose();
            RoomCamera = null;
        }

        if (_selectedObject != null)
        {
            _selectedObject.Dispose();
            _selectedObject = null;
        }

        if (_placedObject != null)
        {
            _placedObject.Dispose();
            _placedObject = null;
        }

        _furnitureDataMap?.Clear();
        _furnitureDataMap = null;

        _wallItemDataMap?.Clear();
        _wallItemDataMap = null;

        if (TileObjectMap != null)
        {
            TileObjectMap.Dispose();
            TileObjectMap = null;
        }
    }

    public void AddFurnitureData(FurnitureData data)
    {
        if (_furnitureDataMap == null)
        {
            return;
        }

        _furnitureDataMap.Remove(data.Id);
        _furnitureDataMap[data.Id] = data;
    }

    public FurnitureData? GetFurnitureData()
    {
        if (_furnitureDataMap == null || _furnitureDataMap.Count == 0)
        {
            return null;
        }

        // Get first key and remove it
        using Dictionary<int, FurnitureData>.Enumerator enumerator = _furnitureDataMap.GetEnumerator();

        if (enumerator.MoveNext())
        {
            return GetFurnitureDataWithId(enumerator.Current.Key);
        }

        return null;
    }

    public FurnitureData? GetFurnitureDataWithId(int id)
    {
        if (_furnitureDataMap == null || !_furnitureDataMap.Remove(id, out FurnitureData? data))
        {
            return null;
        }

        return data;
    }

    public void AddWallItemData(FurnitureData data)
    {
        if (_wallItemDataMap == null)
        {
            return;
        }

        _wallItemDataMap.Remove(data.Id);
        _wallItemDataMap[data.Id] = data;
    }

    public FurnitureData? GetWallItemData()
    {
        if (_wallItemDataMap == null || _wallItemDataMap.Count == 0)
        {
            return null;
        }

        using Dictionary<int, FurnitureData>.Enumerator enumerator = _wallItemDataMap.GetEnumerator();

        if (enumerator.MoveNext())
        {
            return GetWallItemDataWithId(enumerator.Current.Key);
        }

        return null;
    }

    public FurnitureData? GetWallItemDataWithId(int id)
    {
        if (_wallItemDataMap == null || !_wallItemDataMap.Remove(id, out FurnitureData? data))
        {
            return null;
        }

        return data;
    }

    public bool AddButtonMouseCursorOwner(string owner)
    {
        if (_mouseButtonCursorOwners.Contains(owner))
        {
            return false;
        }

        _mouseButtonCursorOwners.Add(owner);

        return true;
    }

    public bool RemoveButtonMouseCursorOwner(string owner)
    {
        return _mouseButtonCursorOwners.Remove(owner);
    }

    public bool HasButtonMouseCursorOwners()
    {
        return _mouseButtonCursorOwners.Count > 0;
    }
}
