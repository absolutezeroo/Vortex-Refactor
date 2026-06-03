using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Room.Renderer;

/// <summary>
/// Room renderer managing multiple canvases and room object display.
/// Implements both the renderer interface and the container interface used by canvases.
/// </summary>
/// @see com.sulake.room.renderer.RoomRenderer (class_3447)
public class RoomRenderer : IRoomRenderer, IRoomSpriteCanvasContainer
{
    private Dictionary<string, IRoomObject>? _objects = new();
    private List<string>? _objectKeys =
        [];
    private Dictionary<string, RoomSpriteCanvas>? _canvases = new();
    private List<string>? _canvasKeys =
        [];

    public bool Disposed { get; private set; }

    public string? RoomObjectVariableAccurateZ { get; set; }

    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }
        if (_canvases != null)
        {
            foreach (RoomSpriteCanvas canvas in _canvases.Values)
            {
                canvas.Dispose();
            }
            _canvases.Clear();
            _canvases = null;
        }
        _canvasKeys = null;
        _objects?.Clear();
        _objects = null;
        _objectKeys = null;
        Disposed = true;
    }

    public void Reset()
    {
        _objects?.Clear();
        _objectKeys?.Clear();
    }

    public string? GetRoomObjectIdentifier(IRoomObject obj)
    {
        if (obj != null)
        {
            return obj.InstanceId.ToString();
        }
        return null;
    }

    public void FeedRoomObject(IRoomObject obj)
    {
        if (obj == null)
        {
            return;
        }
        string key = GetRoomObjectIdentifier(obj) ?? "";
        if (_objects != null && !_objects.ContainsKey(key))
        {
            _objects[key] = obj;
            _objectKeys?.Add(key);
        }
        else if (_objects != null)
        {
            _objects[key] = obj;
        }
    }

    public void RemoveRoomObject(IRoomObject obj)
    {
        string key = GetRoomObjectIdentifier(obj) ?? "";
        _objects?.Remove(key);
        _objectKeys?.Remove(key);

        if (_canvases != null)
        {
            foreach (RoomSpriteCanvas canvas in _canvases.Values)
            {
                canvas.RoomObjectRemoved(key);
            }
        }
    }

    public IRoomObject? GetRoomObject(string identifier)
    {
        if (_objects != null && _objects.TryGetValue(identifier, out IRoomObject? obj))
        {
            return obj;
        }
        return null;
    }

    public IRoomObject? GetRoomObjectWithIndex(int index)
    {
        if (_objectKeys != null && index >= 0 && index < _objectKeys.Count)
        {
            return GetRoomObject(_objectKeys[index]);
        }
        return null;
    }

    public string? GetRoomObjectIdWithIndex(int index)
    {
        if (_objectKeys != null && index >= 0 && index < _objectKeys.Count)
        {
            return _objectKeys[index];
        }
        return null;
    }

    public int GetRoomObjectCount()
    {
        return _objectKeys?.Count ?? 0;
    }

    private void Render()
    {
        int time = System.Environment.TickCount;
        if (_canvasKeys == null)
        {
            return;
        }
        for (int i = _canvasKeys.Count - 1; i >= 0; i--)
        {
            if (_canvases!.TryGetValue(_canvasKeys[i], out RoomSpriteCanvas? canvas))
            {
                canvas.Render(time);
            }
        }
    }

    public IRoomRenderingCanvas? CreateCanvas(int id, int width, int height, int scale)
    {
        string key = id.ToString();
        if (_canvases!.TryGetValue(key, out RoomSpriteCanvas? existing))
        {
            existing.Initialize(width, height);
            if (existing.Geometry is RoomGeometry geometry)
            {
                geometry.Scale = scale;
            }
            return existing;
        }
        RoomSpriteCanvas canvas = CreateCanvasInstance(id, width, height, scale);
        _canvases[key] = canvas;
        _canvasKeys?.Add(key);
        return canvas;
    }

    protected virtual RoomSpriteCanvas CreateCanvasInstance(int id, int width, int height, int scale)
    {
        return new RotatingRoomSpriteCanvas(this, id, width, height, scale);
    }

    public IRoomRenderingCanvas? GetCanvas(int id)
    {
        string key = id.ToString();
        return _canvases!.GetValueOrDefault(key);
    }

    public bool DisposeCanvas(int id)
    {
        string key = id.ToString();
        if (_canvases!.Remove(key, out RoomSpriteCanvas? canvas))
        {
            _canvasKeys?.Remove(key);
            canvas.Dispose();
        }
        return false;
    }

    public void Update(uint time)
    {
        Render();
        if (_canvasKeys != null)
        {
            for (int i = _canvasKeys.Count - 1; i >= 0; i--)
            {
                if (_canvases!.TryGetValue(_canvasKeys[i], out RoomSpriteCanvas? canvas))
                {
                    canvas.Update();
                }
            }
        }
    }
}
