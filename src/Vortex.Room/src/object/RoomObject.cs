using Vortex.Room.Object.Logic;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Room.Object;

/// @see com.sulake.room.object.RoomObject
public class RoomObject : IRoomObjectController
{
    private static int s_instanceCounter;

    private readonly Vector3d _location;
    private readonly Vector3d _direction;
    private readonly Vector3d _locationBuffer;
    private readonly Vector3d _directionBuffer;
    private readonly int[] _states;
    private RoomObjectModel? _model;
    private string? _avatarLibraryAssetName;

    public RoomObject(int id, int stateCount, string type)
    {
        Id = id;
        _location = new Vector3d();
        _direction = new Vector3d();
        _locationBuffer = new Vector3d();
        _directionBuffer = new Vector3d();
        _states = new int[stateCount];
        for (int i = stateCount - 1; i >= 0; i--)
        {
            _states[i] = 0;
        }
        Type = type;
        _model = new RoomObjectModel();
        Visualization = null;
        EventHandler = null;
        UpdateId = 0;
        InstanceId = s_instanceCounter++;
    }

    public int Id { get; }

    public int InstanceId { get; }

    public string Type { get; }

    public bool IsInitialized { get; private set; }

    public int UpdateId { get; private set; }

    public IVector3d Location
    {
        get
        {
            _locationBuffer.Assign(_location);
            return _locationBuffer;
        }
    }

    public IVector3d Direction
    {
        get
        {
            _directionBuffer.Assign(_direction);
            return _directionBuffer;
        }
    }

    public IRoomObjectModel Model => _model!;

    public IRoomObjectModelController ModelController => _model!;

    public IRoomObjectVisualization? Visualization { get; private set; }

    public IRoomObjectMouseHandler? MouseHandler => EventHandler;

    public IRoomObjectEventHandler? EventHandler { get; private set; }

    public string? AvatarLibraryAssetName
    {
        get
        {
            _avatarLibraryAssetName ??= "avatar_" + Id;
            return _avatarLibraryAssetName;
        }
    }

    public void Dispose()
    {
        SetVisualization(null);
        SetEventHandler(null);
        if (_model != null)
        {
            _model.Dispose();
            _model = null;
        }
    }

    public void SetInitialized(bool initialized)
    {
        IsInitialized = initialized;
    }

    public void SetLocation(IVector3d location)
    {
        if (location == null)
        {
            return;
        }
        if (_location.X != location.X || _location.Y != location.Y || _location.Z != location.Z)
        {
            _location.X = location.X;
            _location.Y = location.Y;
            _location.Z = location.Z;
            UpdateId++;
        }
    }

    public void SetDirection(IVector3d direction)
    {
        if (direction == null)
        {
            return;
        }
        if (_direction.X != direction.X || _direction.Y != direction.Y || _direction.Z != direction.Z)
        {
            _direction.X = ((direction.X % 360) + 360) % 360;
            _direction.Y = ((direction.Y % 360) + 360) % 360;
            _direction.Z = ((direction.Z % 360) + 360) % 360;
            UpdateId++;
        }
    }

    public int GetState(int index)
    {
        if (index >= 0 && index < _states.Length)
        {
            return _states[index];
        }
        return -1;
    }

    public bool SetState(int value, int index)
    {
        if (index >= 0 && index < _states.Length)
        {
            if (_states[index] != value)
            {
                _states[index] = value;
                UpdateId++;
            }
            return true;
        }
        return false;
    }

    public void SetVisualization(IRoomObjectVisualization? visualization)
    {
        if (visualization != Visualization)
        {
            Visualization?.Dispose();
            Visualization = visualization;
            if (Visualization != null)
            {
                Visualization.Object = this;
            }
        }
    }

    public void SetEventHandler(IRoomObjectEventHandler? handler)
    {
        if (handler == EventHandler)
        {
            return;
        }
        IRoomObjectEventHandler? old = EventHandler;
        if (old != null)
        {
            EventHandler = null;
            old.Object = null;
        }
        EventHandler = handler;
        if (EventHandler != null)
        {
            EventHandler.Object = this;
        }
    }

    public void TearDown()
    {
        EventHandler?.TearDown();
    }
}
