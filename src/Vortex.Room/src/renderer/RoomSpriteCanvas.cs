using System;

using Godot;

using Vortex.Room.Data;
using Vortex.Room.Events;
using Vortex.Room.Object;
using Vortex.Room.Object.Enum;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Renderer.Cache;
using Vortex.Room.Renderer.Utils;
using Vortex.Room.Utils;

namespace Vortex.Room.Renderer;

/// <summary>
/// Main room sprite rendering canvas. Manages room object visualization, sprite sorting,
/// mouse hit-testing, and the rendering pipeline.
/// </summary>
/// @see com.sulake.room.renderer.RoomSpriteCanvas (class_3650)
public class RoomSpriteCanvas : IRoomRenderingCanvas
{
    private const int SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL = 50;
    private const int FRAME_COUNT_FOR_UPDATE_INTERVAL = 50;
    private const double SLOW_FRAME_UPDATE_INTERVAL = 60;
    private const double FAST_FRAME_UPDATE_INTERVAL = 50;
    private const int MAXIMUM_VALID_FRAME_UPDATE_INTERVAL = 1000;

    private RoomGeometry? _geometry;
    private readonly int _bgColor;

    /// Godot adaptation: SubViewport provides clipping isolation and enables
    /// post-processing shaders on the room canvas without affecting parent UI.
    private SubViewportContainer? _viewportContainer;
    private SubViewport? _viewport;
    private Node2D? _display;

    private readonly List<ExtendedSprite> _sprites = [];
    private readonly List<ExtendedSprite> _spritePool = [];
    private int _totalSpriteCount;

    private Dictionary<string, ObjectMouseData>? _mouseOverData;
    private int _mouseX;
    private int _mouseY;

    private BitmapDataCache? _bitmapDataCache;
    private RoomObjectCache? _roomObjectCache;

    private List<SortableSprite> _sortableSprites;
    private readonly List<Rect2I> _exclusionRectangles = [];

    private IRoomRenderingCanvasMouseListener? _mouseListener;
    private Dictionary<string, RoomSpriteMouseEvent>? _eventCache;
    private int _eventId;

    private int _canvasWidth;
    private int _canvasHeight;
    private int _screenOffsetX;
    private int _screenOffsetY;

    private int _lastRenderedWidth = -1;
    private int _lastRenderedHeight = -1;
    private int _lastRenderTime = -1;

    private double _lastMouseX = -10000000;
    private double _lastMouseY = -10000000;
    private int _mouseEventCount;
    private bool _lastMouseResult;

    private bool _useMask;
    private double _avgFrameInterval;
    private int _frameCounter;
    private bool _runningSlow;
    private bool _skipObjectUpdate;

    private bool _skipVisibilityCheck;
    private readonly bool _hasExclusionRectangles;

    private readonly int _lastSlowFrame;
    private double _avgSpriteCount;
    private int _lastSlowFrameInterval;

    public RoomSpriteCanvas(IRoomSpriteCanvasContainer container, int id, int width, int height, int scale)
    {
        _sortableSprites = [];
        Container = container;
        Id = id;
        DisplayObject = new Node2D();

        // Godot adaptation: SubViewport wraps the canvas Node2D for clipping
        // isolation and future post-processing support.
        _viewportContainer = new SubViewportContainer
        {
            Name = "ViewportContainer", Size = new Vector2(width, height), Stretch = true,
        };
        DisplayObject.AddChild(_viewportContainer);

        _viewport = new SubViewport
        {
            Name = "RoomViewport",
            Size = new Vector2I(width, height),
            TransparentBg = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        _viewportContainer.AddChild(_viewport);

        _display = new Node2D
        {
            Name = "canvas",
        };
        _viewport.AddChild(_display);

        _geometry = new RoomGeometry(scale, new Vector3d(-135, 30, 0), new Vector3d(11, 11, 5), new Vector3d(-135, 0.5, 0));
        _bitmapDataCache = new BitmapDataCache(16, 32, 1);

        string? depthKey = Container?.RoomObjectVariableAccurateZ;
        _eventCache = new Dictionary<string, RoomSpriteMouseEvent>();
        _mouseOverData = new Dictionary<string, ObjectMouseData>();
        _roomObjectCache = new RoomObjectCache(depthKey ?? "");
        Initialize(width, height);
    }

    protected IRoomSpriteCanvasContainer? Container { get; private set; }

    protected int ActiveSpriteCount { get; private set; }

    public int Width => (int)(_canvasWidth * Scale);

    public int Height => (int)(_canvasHeight * Scale);

    public int ScreenOffsetX
    {
        get => _screenOffsetX;
        set
        {
            _mouseX -= value - _screenOffsetX;
            _screenOffsetX = value;
        }
    }

    public int ScreenOffsetY
    {
        get => _screenOffsetY;
        set
        {
            _mouseY -= value - _screenOffsetY;
            _screenOffsetY = value;
        }
    }

    public Node2D? DisplayObject { get; private set; }

    public IRoomGeometry? Geometry => _geometry;

    public IRoomRenderingCanvasMouseListener? MouseListener
    {
        set => _mouseListener = value;
    }

    public bool UseMask
    {
        set => _useMask = value;
    }

    public List<RoomObjectSpriteData>? GetSortableSpriteList()
    {
        return _roomObjectCache?.GetSortableSpriteList();
    }

    public List<ISortableSprite>? GetPlaneSortableSprites()
    {
        List<SortableSprite>? planeSprites = _roomObjectCache?.GetPlaneSortableSprites();
        if (planeSprites == null)
        {
            return null;
        }
        List<ISortableSprite> result = new(planeSprites.Count);
        foreach (SortableSprite s in planeSprites)
        {
            result.Add(s);
        }
        return result;
    }

    public void SetScale(double scale, Vector2? point = null, Vector2? offset = null, bool skipUpdate = false)
    {
        if (DisplayObject == null || _display == null)
        {
            return;
        }
        if (point == null)
        {
            point = new Vector2(_canvasWidth / 2f, _canvasHeight / 2f);
        }
        if (offset == null)
        {
            offset = point;
        }
        Scale = scale;
        if (Scale < 1)
        {
            if (!skipUpdate)
            {
                // No rotation in non-Flash
            }
        }
        ScreenOffsetX = (int)(offset.Value.X - (point.Value.X * scale));
        ScreenOffsetY = (int)(offset.Value.Y - (point.Value.Y * scale));
    }

    public double Scale { get; private set; } = 1;

    public Image? TakeScreenShot()
    {
        _skipVisibilityCheck = true;
        double prevScale = Scale;
        int prevOffsetX = _screenOffsetX;
        int prevOffsetY = _screenOffsetY;
        SetScale(1);
        _screenOffsetX = 0;
        _screenOffsetY = 0;
        Render(-1, true);
        _skipVisibilityCheck = false;
        SetScale(prevScale);
        _screenOffsetX = prevOffsetX;
        _screenOffsetY = prevOffsetY;
        // Godot adaptation: sprites are Sprite2D children in the SubViewport,
        // so we can capture the rendered result directly.
        return _viewport?.GetTexture()?.GetImage();
    }

    public void SkipSpriteVisibilityChecking()
    {
        _skipVisibilityCheck = true;
        Render(-1, true);
    }

    public void ResumeSpriteVisibilityChecking()
    {
        _skipVisibilityCheck = false;
    }

    public int Id { get; }

    public void Dispose()
    {
        CleanSprites(0, true);
        if (_geometry != null)
        {
            _geometry.Dispose();
            _geometry = null;
        }
        if (_bitmapDataCache != null)
        {
            _bitmapDataCache.Dispose();
            _bitmapDataCache = null;
        }
        if (_roomObjectCache != null)
        {
            _roomObjectCache.Dispose();
            _roomObjectCache = null;
        }
        Container = null;
        DisplayObject = null;
        _display = null;
        _viewport = null;
        _viewportContainer = null;
        _sortableSprites = [];
        foreach (ExtendedSprite sprite in _spritePool)
        {
            CleanSprite(sprite, true);
        }
        _spritePool.Clear();
        _eventCache?.Clear();
        _eventCache = null;
        _mouseOverData?.Clear();
        _mouseOverData = null;
        _mouseListener = null;
    }

    public void Initialize(int width, int height)
    {
        if (width < 1)
        {
            width = 1;
        }
        if (height < 1)
        {
            height = 1;
        }
        _canvasWidth = width;
        _canvasHeight = height;

        // Godot adaptation: keep SubViewport size in sync with canvas dimensions.
        if (_viewport != null)
        {
            _viewport.Size = new Vector2I(width, height);
        }
        if (_viewportContainer != null)
        {
            _viewportContainer.Size = new Vector2(width, height);
        }
    }

    public void RoomObjectRemoved(string identifier)
    {
        _roomObjectCache?.RemoveObjectCache(identifier);
    }

    public virtual void Render(int time, bool update = false)
    {
        if (time == -1)
        {
            time = _lastRenderTime + 1;
        }
        _skipObjectUpdate = !_skipObjectUpdate;

        if (Container == null || _geometry == null)
        {
            return;
        }
        if (time == _lastRenderTime)
        {
            return;
        }

        CalculateUpdateInterval(time);
        _bitmapDataCache?.Compress();

        int objectCount = Container.GetRoomObjectCount();
        int spriteIndex = 0;

        if (_canvasWidth != _lastRenderedWidth || _canvasHeight != _lastRenderedHeight)
        {
            update = true;
        }

        for (int i = 0; i < objectCount; i++)
        {
            IRoomObject? obj = Container.GetRoomObjectWithIndex(i);

            if (obj == null)
            {
                continue;
            }

            string identifier = Container.GetRoomObjectIdWithIndex(i) ?? "";
            spriteIndex += RenderObject(obj, identifier, time, update, spriteIndex);
        }

        _sortableSprites.Sort((a, b) => b.Z.CompareTo(a.Z));
        if (spriteIndex < _sortableSprites.Count)
        {
            _sortableSprites.RemoveRange(spriteIndex, _sortableSprites.Count - spriteIndex);
        }

        for (int i = 0; i < spriteIndex; i++)
        {
            SortableSprite sortable = _sortableSprites[i];
            UpdateSprite(i, sortable);
        }

        // Godot adaptation: sync z-order of Sprite2D children to match the sorted sprite list.
        // The sort is descending Z (back-to-front), so index 0 = furthest back.
        // Use ZIndex so that sprites render in correct depth order.
        if (_display != null)
        {
            for (int i = 0; i < spriteIndex; i++)
            {
                ExtendedSprite? ext = GetSprite(i);
                if (ext?.DisplaySprite != null)
                {
                    ext.DisplaySprite.ZIndex = i;
                }
            }
        }

        CleanSprites(spriteIndex);
        _lastRenderTime = time;
        _lastRenderedWidth = _canvasWidth;
        _lastRenderedHeight = _canvasHeight;
    }

    private void CalculateUpdateInterval(int time)
    {
        if (_lastRenderTime > 0)
        {
            int interval = time - _lastRenderTime;
            if (interval > 60 * 3)
            {
                Logger.Warn($"Really slow frame update {interval}ms");
                _lastSlowFrameInterval = interval;
            }
            if (interval <= MAXIMUM_VALID_FRAME_UPDATE_INTERVAL)
            {
                _frameCounter++;
                if (_frameCounter == SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL + 1)
                {
                    _avgFrameInterval = interval;
                    _avgSpriteCount = _lastSlowFrame;
                }
                else if (_frameCounter > SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL + 1)
                {
                    double n = _frameCounter - SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL;
                    _avgFrameInterval = (_avgFrameInterval * (n - 1) / n) + ((double)interval / n);
                    _avgSpriteCount = (_avgSpriteCount * (n - 1) / n) + ((double)_lastSlowFrame / n);
                    if (_frameCounter > SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL + FRAME_COUNT_FOR_UPDATE_INTERVAL)
                    {
                        _frameCounter = SKIP_FRAME_COUNT_FOR_UPDATE_INTERVAL;
                        if (!_runningSlow && _avgFrameInterval > SLOW_FRAME_UPDATE_INTERVAL)
                        {
                            _runningSlow = true;
                            Logger.Warn("Room canvas updating really slow - now entering frame skipping mode...");
                        }
                        else if (_runningSlow && _avgFrameInterval < FAST_FRAME_UPDATE_INTERVAL)
                        {
                            _runningSlow = false;
                            Logger.Info("Room canvas updating fast again - now entering normal frame mode...");
                        }
                        _lastSlowFrameInterval = 0;
                    }
                }
            }
        }
    }

    protected RoomObjectCacheItem GetRoomObjectCacheItem(string identifier)
    {
        return _roomObjectCache!.GetObjectCache(identifier);
    }

    private int RenderObject(IRoomObject obj, string identifier, int time, bool forceUpdate, int globalIndex)
    {
        if (obj.Visualization is not IRoomObjectSpriteVisualization visualization)
        {
            _roomObjectCache?.RemoveObjectCache(identifier);
            return 0;
        }

        RoomObjectCacheItem cacheItem = GetRoomObjectCacheItem(identifier);
        cacheItem.ObjectId = obj.Id;

        RoomObjectLocationCacheItem locationCache = cacheItem.Location!;
        RoomObjectSortableSpriteCacheItem spriteCache = cacheItem.Sprites!;

        IVector3d? screenLocation = locationCache.GetScreenLocation(obj, _geometry!);
        if (screenLocation == null)
        {
            _roomObjectCache?.RemoveObjectCache(identifier);
            return 0;
        }

        visualization.Update(_geometry!, time, !spriteCache.IsEmpty || forceUpdate, _skipObjectUpdate && _runningSlow);

        bool locationChanged = locationCache.LocationChanged;
        if (locationChanged)
        {
            forceUpdate = true;
        }

        if (!spriteCache.NeedsUpdate(visualization.InstanceId, visualization.UpdateId) && !forceUpdate)
        {
            return spriteCache.SpriteCount;
        }

        int spriteCount = visualization.SpriteCount;
        int screenX = (int)screenLocation.X;
        int screenY = (int)screenLocation.Y;
        double screenZ = screenLocation.Z;

        if (screenX > 0)
        {
            screenZ += screenX * 1.2e-7;
        }
        else
        {
            screenZ += -screenX * 1.2e-7;
        }

        screenX += _canvasWidth / 2;
        screenY += _canvasHeight / 2;

        int addedCount = 0;

        for (int i = 0; i < spriteCount; i++)
        {
            IRoomObjectSprite? sprite = visualization.GetSprite(i);
            if (sprite == null || !sprite.Visible)
            {
                continue;
            }
            Image? asset = sprite.Asset;
            if (asset == null)
            {
                continue;
            }

            int spriteX = screenX + sprite.OffsetX + _screenOffsetX;
            int spriteY = screenY + sprite.OffsetY + _screenOffsetY;

            if (!RectangleVisible(spriteX, spriteY, asset.GetWidth(), asset.GetHeight()))
            {
                continue;
            }

            SortableSprite? sortable = spriteCache.SpriteCount > addedCount ? spriteCache.GetSprite(addedCount) : null;
            if (sortable == null)
            {
                sortable = new SortableSprite();
                spriteCache.AddSprite(sortable);
                _sortableSprites.Add(sortable);
                sortable.Name = identifier;
            }

            sortable.Sprite = sprite;
            if (sprite.SpriteType == RoomObjectSpriteType.AVATAR || sprite.SpriteType == RoomObjectSpriteType.AVATAR_OWN)
            {
                sprite.LibraryAssetName = obj.AvatarLibraryAssetName ?? "";
            }

            sortable.X = spriteX - _screenOffsetX;
            sortable.Y = spriteY - _screenOffsetY;
            sortable.Z = screenZ + sprite.RelativeDepth + (3.7e-11 * globalIndex);
            addedCount++;
            globalIndex++;
        }

        spriteCache.SetSpriteCount(addedCount);
        return addedCount;
    }

    private bool RectangleVisible(int x, int y, int width, int height)
    {
        if (_skipVisibilityCheck)
        {
            return true;
        }
        x = (int)((x - _screenOffsetX) * Scale) + _screenOffsetX;
        y = (int)((y - _screenOffsetY) * Scale) + _screenOffsetY;
        width = (int)(width * Scale);
        height = (int)(height * Scale);
        if (x < _canvasWidth && x + width >= 0 && y < _canvasHeight && y + height >= 0)
        {
            if (!_hasExclusionRectangles)
            {
                return true;
            }
            return RectangleVisibleWithExclusion(x, y, width, height);
        }
        return false;
    }

    private bool RectangleVisibleWithExclusion(int x, int y, int width, int height)
    {
        if (x < 0)
        {
            width += x;
            x = 0;
        }
        if (y < 0)
        {
            height += y;
            y = 0;
        }
        if (x + width >= _canvasWidth)
        {
            width -= _canvasWidth + 1 - (x + width);
        }
        if (y + height >= _canvasHeight)
        {
            height -= _canvasHeight + 1 - (y + height);
        }
        foreach (Rect2I rect in _exclusionRectangles)
        {
            if (x >= rect.Position.X && x + width < rect.End.X && y >= rect.Position.Y && y + height < rect.End.Y)
            {
                return false;
            }
        }
        return true;
    }

    protected ExtendedSprite? GetSprite(int index)
    {
        if (index < 0 || index >= _totalSpriteCount)
        {
            return null;
        }
        return _sprites[index];
    }

    private void CreateSprite(SortableSprite sortable, int insertIndex = -1)
    {
        IRoomObjectSprite? sprite = sortable.Sprite;
        if (sprite == null)
        {
            return;
        }

        ExtendedSprite? extSprite = null;
        if (_spritePool.Count > 0)
        {
            extSprite = _spritePool[^1];
            _spritePool.RemoveAt(_spritePool.Count - 1);
        }
        extSprite ??= new ExtendedSprite();

        extSprite.X = sortable.X;
        extSprite.Y = sortable.Y;
        extSprite.OffsetRefX = sprite.OffsetX;
        extSprite.OffsetRefY = sprite.OffsetY;
        extSprite.Identifier = sortable.Name;
        extSprite.Alpha = sprite.Alpha / 255.0;
        extSprite.Tag = sprite.Tag;
        extSprite.BlendMode = sprite.BlendMode ?? "normal";
        extSprite.VaryingDepth = sprite.VaryingDepth;
        extSprite.ClickHandling = sprite.ClickHandling;
        extSprite.SkipMouseHandling = sprite.SkipMouseHandling;
        extSprite.BitmapData = GetBitmapData(sprite.Asset, sprite.AssetName ?? "", sprite.FlipH, sprite.FlipV, sprite.Color);
        UpdateEnterRoomEffect(extSprite, sprite);
        extSprite.AlphaTolerance = sprite.AlphaTolerance;

        // Godot adaptation: create a Sprite2D child in _display for visual rendering.
        // Mirrors AS3 _display.addChild(bitmap).
        if (_display != null)
        {
            if (extSprite.DisplaySprite == null)
            {
                extSprite.DisplaySprite = new Sprite2D
                {
                    Centered = false
                };
                _display.AddChild(extSprite.DisplaySprite);
            }
            SyncSpriteDisplay(extSprite);
            if (insertIndex >= 0 && insertIndex < _display.GetChildCount())
            {
                _display.MoveChild(extSprite.DisplaySprite, insertIndex);
            }
        }

        if (insertIndex < 0 || insertIndex >= _totalSpriteCount)
        {
            _sprites.Add(extSprite);
            _totalSpriteCount++;
        }
        else
        {
            _sprites.Insert(insertIndex, extSprite);
        }
        ActiveSpriteCount++;
    }

    private bool UpdateSprite(int index, SortableSprite sortable)
    {
        if (index >= _totalSpriteCount)
        {
            CreateSprite(sortable);
            return true;
        }

        IRoomObjectSprite? sprite = sortable.Sprite;
        ExtendedSprite? extSprite = GetSprite(index);
        if (extSprite != null && sprite != null)
        {
            if (extSprite.VaryingDepth != sprite.VaryingDepth)
            {
                if (extSprite.VaryingDepth && !sprite.VaryingDepth)
                {
                    // Sprite is being recycled to pool — hide its display node
                    if (extSprite.DisplaySprite != null)
                    {
                        extSprite.DisplaySprite.Visible = false;
                    }
                    _sprites.RemoveAt(index);
                    _totalSpriteCount--;
                    _spritePool.Add(extSprite);
                    return UpdateSprite(index, sortable);
                }
                CreateSprite(sortable, index);
                return true;
            }

            if (extSprite.NeedsUpdate(sprite.InstanceId, sprite.UpdateId) || RoomEnterEffect.IsVisualizationOn())
            {
                extSprite.AlphaTolerance = sprite.AlphaTolerance;
                double alpha = sprite.Alpha / 255.0;
                if (Math.Abs(extSprite.Alpha - alpha) > 0.001)
                {
                    extSprite.Alpha = alpha;
                }
                extSprite.Identifier = sortable.Name;
                extSprite.Tag = sprite.Tag;
                extSprite.VaryingDepth = sprite.VaryingDepth;
                extSprite.BlendMode = sprite.BlendMode ?? "normal";
                extSprite.ClickHandling = sprite.ClickHandling;
                extSprite.SkipMouseHandling = sprite.SkipMouseHandling;

                ExtendedBitmapData? bitmapData =
                    GetBitmapData(sprite.Asset, sprite.AssetName ?? "", sprite.FlipH, sprite.FlipV, sprite.Color);
                if (extSprite.BitmapData != bitmapData)
                {
                    extSprite.BitmapData = bitmapData;
                }
                UpdateEnterRoomEffect(extSprite, sprite);
            }

            if (extSprite.X != sortable.X)
            {
                extSprite.X = sortable.X;
            }
            if (extSprite.Y != sortable.Y)
            {
                extSprite.Y = sortable.Y;
            }
            extSprite.OffsetRefX = sprite.OffsetX;
            extSprite.OffsetRefY = sprite.OffsetY;
            SyncSpriteDisplay(extSprite);
            return true;
        }
        return false;
    }

    private static void UpdateEnterRoomEffect(ExtendedSprite extSprite, IRoomObjectSprite sprite)
    {
        if (!RoomEnterEffect.IsVisualizationOn() || extSprite.BitmapData == null)
        {
            return;
        }
        if (sprite.SpriteType == RoomObjectSpriteType.AVATAR_OWN)
        {
            // No effect for own avatar
        }
        else if (sprite.SpriteType == RoomObjectSpriteType.ROOM_PLANE)
        {
            extSprite.Alpha = RoomEnterEffect.GetDelta(0.9);
        }
        else if (sprite.SpriteType == RoomObjectSpriteType.AVATAR)
        {
            extSprite.Alpha = RoomEnterEffect.GetDelta(0.5);
        }
        else
        {
            extSprite.Alpha = RoomEnterEffect.GetDelta(0.1);
        }
    }

    private void CleanSprites(int keepCount, bool fullDispose = false)
    {
        if (keepCount < 0)
        {
            keepCount = 0;
        }
        if (keepCount < ActiveSpriteCount || ActiveSpriteCount == 0)
        {
            for (int i = _totalSpriteCount - 1; i >= keepCount; i--)
            {
                if (i < _sprites.Count)
                {
                    CleanSprite(_sprites[i], fullDispose);
                }
            }
        }
        ActiveSpriteCount = keepCount;
    }

    private static void CleanSprite(ExtendedSprite? sprite, bool fullDispose)
    {
        if (sprite != null)
        {
            if (!fullDispose)
            {
                sprite.BitmapData = null;
                // Hide the Sprite2D but keep it alive for pooled reuse
                if (sprite.DisplaySprite != null)
                {
                    sprite.DisplaySprite.Visible = false;
                }
            }
            else
            {
                sprite.Dispose();
            }
        }
    }

    private ExtendedBitmapData? GetBitmapData(Image? asset, string assetName, bool flipH, bool flipV, int color)
    {
        color &= 0xFFFFFF;
        if (!flipH && !flipV && color == 0xFFFFFF)
        {
            return WrapImage(asset);
        }

        ExtendedBitmapData? result = null;
        string cacheKey;

        if ((flipH || flipV) && color != 0xFFFFFF)
        {
            cacheKey = assetName + " " + color + (flipH ? " FH" : "") + (flipV ? " FV" : "");
            if (assetName.Length > 0)
            {
                result = _bitmapDataCache?.GetBitmapData(cacheKey);
            }
            if (result == null)
            {
                ExtendedBitmapData? colored = GetColoredBitmapData(WrapImage(asset), assetName, color);
                if (colored != null)
                {
                    result = GetFlippedBitmapData(colored, assetName, true, flipH, flipV);
                    if (assetName.Length > 0)
                    {
                        _bitmapDataCache?.AddBitmapData(cacheKey, result!);
                    }
                    return result;
                }
                ExtendedBitmapData? flipped = GetFlippedBitmapData(WrapImage(asset), assetName, true, flipH, flipV);
                if (flipped != null)
                {
                    result = GetColoredBitmapData(flipped, "", color, true);
                    if (assetName.Length > 0)
                    {
                        _bitmapDataCache?.AddBitmapData(cacheKey, result!);
                    }
                    return result;
                }
                result = GetColoredBitmapData(WrapImage(asset), assetName, color, true);
                result = GetFlippedBitmapData(result, assetName, true, flipH, flipV);
                if (assetName.Length > 0)
                {
                    _bitmapDataCache?.AddBitmapData(cacheKey, result!);
                }
            }
        }
        else if (flipH || flipV)
        {
            result = GetFlippedBitmapData(WrapImage(asset), assetName, true, flipH, flipV);
        }
        else
        {
            if (color == 0xFFFFFF)
            {
                return WrapImage(asset);
            }
            result = GetColoredBitmapData(WrapImage(asset), assetName, color, true);
        }
        return result;
    }

    private ExtendedBitmapData? GetFlippedBitmapData(ExtendedBitmapData? source, string assetName, bool createIfMissing, bool flipH,
        bool flipV)
    {
        if (source?.Data == null)
        {
            return null;
        }
        string cacheKey = assetName + (flipH ? " FH" : "") + (flipV ? " FV" : "");
        ExtendedBitmapData? result = null;
        if (assetName.Length > 0)
        {
            result = _bitmapDataCache?.GetBitmapData(cacheKey);
            if (!createIfMissing)
            {
                return result;
            }
        }
        if (result == null)
        {
            Image? srcImage = source.Data;
            int w = srcImage.GetWidth();
            int h = srcImage.GetHeight();
            if (w <= 0 || h <= 0)
            {
                return null;
            }
            Image? flipped = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int srcX = flipH ? w - 1 - x : x;
                    int srcY = flipV ? h - 1 - y : y;
                    flipped.SetPixel(x, y, srcImage.GetPixel(srcX, srcY));
                }
            }
            result = new ExtendedBitmapData(w, h);
            result.Data?.BlitRect(flipped, new Rect2I(0, 0, w, h), Vector2I.Zero);
            if (assetName.Length > 0)
            {
                _bitmapDataCache?.AddBitmapData(cacheKey, result);
            }
        }
        return result;
    }

    private ExtendedBitmapData? GetColoredBitmapData(ExtendedBitmapData? source, string assetName, int color,
        bool createIfMissing = false)
    {
        if (source?.Data == null)
        {
            return null;
        }
        string cacheKey = assetName + " " + color;
        ExtendedBitmapData? result = null;
        if (assetName.Length > 0)
        {
            result = _bitmapDataCache?.GetBitmapData(cacheKey);
            if (!createIfMissing)
            {
                return result;
            }
        }
        if (result == null)
        {
            Image? srcImage = source.Data;
            int w = srcImage.GetWidth();
            int h = srcImage.GetHeight();
            if (w <= 0 || h <= 0)
            {
                return null;
            }

            float redMul = ((color >> 16) & 0xFF) / 255.0f;
            float greenMul = ((color >> 8) & 0xFF) / 255.0f;
            float blueMul = (color & 0xFF) / 255.0f;

            Image? colored = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
            colored.BlitRect(srcImage, new Rect2I(0, 0, w, h), Vector2I.Zero);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color pixel = colored.GetPixel(x, y);
                    colored.SetPixel(x, y, new Color(pixel.R * redMul, pixel.G * greenMul, pixel.B * blueMul, pixel.A));
                }
            }
            result = new ExtendedBitmapData(w, h);
            result.Data?.BlitRect(colored, new Rect2I(0, 0, w, h), Vector2I.Zero);
            if (assetName.Length > 0)
            {
                _bitmapDataCache?.AddBitmapData(cacheKey, result);
            }
        }
        return result;
    }

    private static ExtendedBitmapData? WrapImage(Image? image)
    {
        if (image == null)
        {
            return null;
        }
        ExtendedBitmapData wrapped = new(image.GetWidth(), image.GetHeight());
        wrapped.Data?.BlitRect(image, new Rect2I(0, 0, image.GetWidth(), image.GetHeight()), Vector2I.Zero);
        return wrapped;
    }

    /// <summary>
    /// Synchronizes the visual state of a Godot Sprite2D from its ExtendedSprite data.
    /// Mirrors the AS3 pattern where Bitmap display properties were set after sprite population.
    /// </summary>
    private static void SyncSpriteDisplay(ExtendedSprite ext)
    {
        Sprite2D? sprite2D = ext.DisplaySprite;
        if (sprite2D == null)
        {
            return;
        }

        sprite2D.Position = new Vector2(ext.X, ext.Y);
        sprite2D.Modulate = new Color(1, 1, 1, (float)ext.Alpha);
        sprite2D.Visible = true;

        // Blend mode mapping: AS3 BlendMode string → Godot CanvasItemMaterial.BlendModeEnum
        if (ext.BlendMode == "add")
        {
            sprite2D.Material ??= new CanvasItemMaterial();
            ((CanvasItemMaterial)sprite2D.Material).BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
        }
        else if (ext.BlendMode == "multiply")
        {
            sprite2D.Material ??= new CanvasItemMaterial();
            ((CanvasItemMaterial)sprite2D.Material).BlendMode = CanvasItemMaterial.BlendModeEnum.Mul;
        }
        else
        {
            // "normal" / default — clear material to use default Mix blend
            if (sprite2D.Material is CanvasItemMaterial mat && mat.BlendMode != CanvasItemMaterial.BlendModeEnum.Mix)
            {
                sprite2D.Material = null;
            }
        }

        // Texture sync: create or update ImageTexture from bitmap data
        Image? image = ext.BitmapData?.Data;
        if (image == null)
        {
            sprite2D.Texture = null;
            ext.DisplayTexture = null;
            return;
        }

        if (ext.DisplayTexture == null
            || ext.DisplayTexture.GetWidth() != image.GetWidth()
            || ext.DisplayTexture.GetHeight() != image.GetHeight())
        {
            ext.DisplayTexture = ImageTexture.CreateFromImage(image);
            sprite2D.Texture = ext.DisplayTexture;
        }
        else
        {
            ext.DisplayTexture.Update(image);
        }
    }

    protected static string GetObjectId(ExtendedSprite? sprite)
    {
        if (sprite != null)
        {
            return sprite.Identifier;
        }
        return "";
    }

    public bool HandleMouseEvent(int x, int y, string type, bool altKey, bool ctrlKey, bool shiftKey, bool buttonDown)
    {
        x -= _screenOffsetX;
        y -= _screenOffsetY;
        _mouseX = (int)(x / Scale);
        _mouseY = (int)(y / Scale);
        if (_mouseEventCount > 0 && type == "mouseMove")
        {
            return _lastMouseResult;
        }
        _lastMouseResult = CheckMouseHits((int)(x / Scale), (int)(y / Scale), type, altKey, ctrlKey, shiftKey, buttonDown);
        _mouseEventCount++;
        return _lastMouseResult;
    }

    protected RoomSpriteMouseEvent CreateMouseEvent(int screenX, int screenY, int localX, int localY,
        string type, string spriteTag, bool altKey, bool ctrlKey, bool shiftKey, bool buttonDown)
    {
        double canvasX = screenX - (_canvasWidth / 2.0);
        double canvasY = screenY - (_canvasHeight / 2.0);
        string canvasIdStr = "canvas_" + Id;
        return new RoomSpriteMouseEvent(type, canvasIdStr + "_" + _eventId, canvasIdStr, spriteTag,
            canvasX, canvasY, localX, localY, ctrlKey, altKey, shiftKey, buttonDown);
    }

    private bool CheckMouseClickHits(double x, double y, bool doubleClick,
        bool altKey = false, bool ctrlKey = false, bool shiftKey = false, bool buttonDown = false)
    {
        string type = doubleClick ? "doubleClick" : "click";
        List<string> processedObjects = new();
        bool hasClickHandler = false;

        for (int i = ActiveSpriteCount - 1; i >= 0; i--)
        {
            ExtendedSprite? sprite = GetSprite(i);
            if (sprite is
                {
                    ClickHandling: true,
                })
            {
                if (sprite.HitTest((int)(x - sprite.X), (int)(y - sprite.Y)))
                {
                    string objectId = GetObjectId(sprite);
                    if (!processedObjects.Contains(objectId))
                    {
                        string tag = sprite.Tag;
                        RoomSpriteMouseEvent mouseEvent = CreateMouseEvent((int)x, (int)y, (int)(x - sprite.X), (int)(y - sprite.Y),
                            type, tag, altKey, ctrlKey, shiftKey, buttonDown);
                        BufferMouseEvent(mouseEvent, objectId);
                        processedObjects.Add(objectId);
                    }
                }
                hasClickHandler = true;
            }
        }
        ProcessMouseEvents();
        return hasClickHandler;
    }

    private bool CheckMouseHits(int x, int y, string type,
        bool altKey = false, bool ctrlKey = false, bool shiftKey = false, bool buttonDown = false)
    {
        List<string> processedObjects = new();
        bool hasHit = false;

        for (int i = ActiveSpriteCount - 1; i >= 0; i--)
        {
            ExtendedSprite? sprite = GetSprite(i);
            if (sprite != null && sprite.HitTest(x - sprite.X, y - sprite.Y))
            {
                if (sprite.SkipMouseHandling)
                {
                    continue;
                }
                if (sprite.ClickHandling && type is "click" or "doubleClick")
                {
                    continue;
                }

                string objectId = GetObjectId(sprite);
                if (processedObjects.Contains(objectId))
                {
                    continue;
                }

                string tag = sprite.Tag;
                ObjectMouseData? mouseData = null;
                _mouseOverData?.TryGetValue(objectId, out mouseData);

                if (mouseData != null && mouseData.SpriteTag != tag)
                {
                    RoomSpriteMouseEvent rollOutEvent = CreateMouseEvent(0, 0, 0, 0, "rollOut", mouseData.SpriteTag, altKey, ctrlKey,
                        shiftKey, buttonDown);
                    BufferMouseEvent(rollOutEvent, objectId);
                }

                RoomSpriteMouseEvent mouseEvent;
                if (type == "mouseMove" && (mouseData == null || mouseData.SpriteTag != tag))
                {
                    mouseEvent = CreateMouseEvent(x, y, x - sprite.X, y - sprite.Y, "rollOver", tag, altKey, ctrlKey, shiftKey,
                        buttonDown);
                }
                else
                {
                    mouseEvent = CreateMouseEvent(x, y, x - sprite.X, y - sprite.Y, type, tag, altKey, ctrlKey, shiftKey, buttonDown);
                    mouseEvent.SpriteOffsetX = sprite.OffsetRefX;
                    mouseEvent.SpriteOffsetY = sprite.OffsetRefY;
                }

                if (mouseData == null)
                {
                    mouseData = new ObjectMouseData
                    {
                        ObjectId = objectId,
                    };
                    _mouseOverData?.TryAdd(objectId, mouseData);
                }
                mouseData.SpriteTag = tag;

                if (type != "mouseMove" || x != _lastMouseX || y != _lastMouseY)
                {
                    BufferMouseEvent(mouseEvent, objectId);
                }
                processedObjects.Add(objectId);
                hasHit = true;
            }
        }

        // Send rollOut for objects no longer under cursor
        if (_mouseOverData != null)
        {
            List<string> toRemove = new();
            foreach ((string key, ObjectMouseData data) in _mouseOverData)
            {
                if (!processedObjects.Contains(key))
                {
                    RoomSpriteMouseEvent rollOutEvent = CreateMouseEvent(0, 0, 0, 0, "rollOut", data.SpriteTag, altKey, ctrlKey,
                        shiftKey, buttonDown);
                    BufferMouseEvent(rollOutEvent, key);
                    toRemove.Add(key);
                }
            }
            foreach (string key in toRemove)
            {
                _mouseOverData.Remove(key);
            }
        }

        ProcessMouseEvents();
        _lastMouseX = x;
        _lastMouseY = y;
        return hasHit;
    }

    protected virtual void BufferMouseEvent(RoomSpriteMouseEvent mouseEvent, string objectId)
    {
        if (_eventCache != null)
        {
            _eventCache[objectId] = mouseEvent;
        }
    }

    protected virtual void ProcessMouseEvents()
    {
        if (Container == null || _eventCache == null)
        {
            return;
        }

        foreach ((string objectId, RoomSpriteMouseEvent mouseEvent) in _eventCache)
        {
            IRoomObject? roomObj = Container.GetRoomObject(objectId);
            if (roomObj == null)
            {
                continue;
            }
            if (_mouseListener != null)
            {
                _mouseListener.ProcessRoomCanvasMouseEvent(mouseEvent, roomObj, Geometry!);
            }
            else
            {
                roomObj.MouseHandler?.MouseEvent(mouseEvent, _geometry!);
            }
        }
        _eventCache?.Clear();
    }

    public void Update()
    {
        if (_mouseEventCount == 0)
        {
            CheckMouseHits(_mouseX, _mouseY, "mouseMove");
        }
        _mouseEventCount = 0;
        _eventId++;
    }
}
