using System;
using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Enum;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.RoomVisualization
public class RoomVisualization : RoomObjectSpriteVisualization, IRoomPlaneContainer
{
    public const uint FLOOR_COLOR = 0xFFFFFF;
    public const uint FLOOR_COLOR_LEFT = 0xDDDDDD;
    public const uint FLOOR_COLOR_RIGHT = 0xBBBBBB;

    private const uint WALL_COLOR_TOP = 0xFFFFFF;
    private const uint WALL_COLOR_SIDE = 0xCCCCCC;
    private const uint WALL_COLOR_BOTTOM = 0x999999;
    private const uint WALL_COLOR_BORDER = 0x999999;

    public const uint LANDSCAPE_COLOR_TOP = 0xFFFFFF;
    public const uint LANDSCAPE_COLOR_SIDE = 0xCCCCCC;
    public const uint LANDSCAPE_COLOR_BOTTOM = 0x999999;

    private const double ROOM_DEPTH_OFFSET = 1000;
    private const int UPDATE_INTERVAL = 250;

    protected RoomVisualizationData? _visualizationData;
    private RoomPlaneParser? _planeParser;
    private List<RoomPlane> _planes;
    private Dictionary<int, int> _planeMap;
    private bool _planesInitialized;
    private List<RoomPlane> _visiblePlanes;
    private List<int> _visiblePlaneSpriteNumbers;
    private RoomPlaneBitmapMaskParser? _maskParser;
    private string? _wallType;
    private string? _floorType;
    private string? _landscapeType;
    private double _floorThickness = double.NaN;
    private double _wallThickness = double.NaN;
    private double _floorHoleUpdateTime = double.NaN;
    private string? _maskXml;
    private uint _backgroundColor = 0xFFFFFF;
    private int _backgroundRed = 255;
    private int _backgroundGreen = 255;
    private int _backgroundBlue = 255;
    private int _renderCount;
    private int _lastUpdateTime = -1000;
    private int _geometryUpdateId = -1;
    private int _previousModelUpdateId = -1;

    private double _dirX;
    private double _dirY;
    private double _dirZ;
    private double _cachedScale;

    private readonly bool[] _typeVisibilities;

    public RoomVisualization()
    {
        _planes = [];
        _planeMap = [];
        _visiblePlanes = [];
        _visiblePlaneSpriteNumbers = [];
        _planeParser = new RoomPlaneParser();
        _maskParser = new RoomPlaneBitmapMaskParser();

        _typeVisibilities = new bool[4];
        _typeVisibilities[RoomPlane.TYPE_UNDEFINED] = false;
        _typeVisibilities[RoomPlane.TYPE_FLOOR] = true;
        _typeVisibilities[RoomPlane.TYPE_WALL] = true;
        _typeVisibilities[RoomPlane.TYPE_LANDSCAPE] = true;
    }

    public double FloorRelativeDepth => ROOM_DEPTH_OFFSET + 0.1;

    public double WallRelativeDepth => ROOM_DEPTH_OFFSET + 0.5;

    public double WallAdRelativeDepth => ROOM_DEPTH_OFFSET + 0.49;

    public int PlaneCount => _planes.Count;

    public List<IRoomPlane> Planes
    {
        get
        {
            List<IRoomPlane> result = [];

            result.AddRange(_visiblePlanes.Cast<IRoomPlane>());

            return result;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        ResetRoomPlanes();
        _planes = null!;
        _planeMap = null!;
        _visiblePlanes = null!;
        _visiblePlaneSpriteNumbers = null!;

        if (_planeParser != null)
        {
            _planeParser.Dispose();
            _planeParser = null;
        }

        if (_maskParser != null)
        {
            _maskParser.Dispose();
            _maskParser = null;
        }

        if (_visualizationData != null)
        {
            _visualizationData.ClearCache();
            _visualizationData = null;
        }
    }

    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        Reset();

        if (data is not RoomVisualizationData vizData)
        {
            return false;
        }

        _visualizationData = vizData;
        _visualizationData.InitializeAssetCollection(AssetCollection);

        return true;
    }

    public override void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
        IRoomObject? obj = Object;

        if (obj == null || geometry == null)
        {
            return;
        }

        bool geometryChanged = UpdateGeometry(geometry);
        IRoomObjectModel model = obj.Model;

        bool dirty = false;

        if (UpdatePlaneThicknesses(model))
        {
            dirty = true;
        }

        if (UpdateFloorHoles(model))
        {
            dirty = true;
        }

        InitializeRoomPlanes();
        dirty = UpdateMasksAndColors(model);

        if (time < _lastUpdateTime + UPDATE_INTERVAL && !geometryChanged && !dirty)
        {
            return;
        }

        if (UpdatePlaneTexturesAndVisibilities(model))
        {
            if (!dirty)
            {
                dirty = true;
            }
        }

        if (UpdatePlanes(geometry, geometryChanged, time))
        {
            if (!dirty)
            {
                dirty = true;
            }
        }

        if (dirty)
        {
            for (int i = 0; i < _visiblePlanes.Count; i++)
            {
                int spriteIndex = _visiblePlaneSpriteNumbers[i];
                IRoomObjectSprite? sprite = GetSprite(spriteIndex);
                RoomPlane plane = _visiblePlanes[i];

                if (sprite == null || plane.Type == RoomPlane.TYPE_LANDSCAPE)
                {
                    continue;
                }

                uint c = plane.Color;
                int b = (int)(c & 0xFF) * _backgroundBlue / 255;
                int g = (int)((c >> 8) & 0xFF) * _backgroundGreen / 255;
                int r = (int)((c >> 16) & 0xFF) * _backgroundRed / 255;
                int a = (int)(c >> 24);
                c = (uint)((a << 24) + (r << 16) + (g << 8) + b);
                sprite.Color = (int)c;
            }

            IncreaseUpdateId();
        }

        _previousModelUpdateId = model.UpdateId;
        _lastUpdateTime = time;
    }

    protected void DefineSprites(int startIndex = 0)
    {
        int count = _planes.Count;
        CreateSprites(count);

        for (int i = startIndex; i < count; i++)
        {
            RoomPlane plane = _planes[i];
            IRoomObjectSprite? sprite = GetSprite(i);

            if (sprite == null || plane.LeftSide == null || plane.RightSide == null)
            {
                continue;
            }

            if (plane.Type == RoomPlane.TYPE_WALL &&
                (plane.LeftSide.Length < 1 || plane.RightSide.Length < 1))
            {
                sprite.AlphaTolerance = 256;
            }
            else
            {
                sprite.AlphaTolerance = 128;
            }

            sprite.Tag = plane.Type switch
            {
                RoomPlane.TYPE_WALL => "plane.wall@" + (i + 1),
                RoomPlane.TYPE_FLOOR => "plane.floor@" + (i + 1),
                _ => "plane@" + (i + 1),
            };

            sprite.SpriteType = RoomObjectSpriteType.ROOM_PLANE;
        }
    }

    protected void InitializeRoomPlanes()
    {
        if (_planesInitialized)
        {
            return;
        }

        IRoomObject? obj = Object;

        if (obj == null || _planeParser == null)
        {
            return;
        }

        if (!double.IsNaN(_floorThickness))
        {
            _planeParser.FloorThicknessMultiplier = _floorThickness;
        }

        if (!double.IsNaN(_wallThickness))
        {
            _planeParser.WallThicknessMultiplier = _wallThickness;
        }

        string? planeXml = obj.Model.GetString("room_plane_xml");

        if (string.IsNullOrEmpty(planeXml))
        {
            return;
        }

        _planeParser.ClearHighlightArea();

        if (!_planeParser.InitializeFromXML(XElement.Parse(planeXml)))
        {
            return;
        }

        CreatePlanesAndSprites();
    }

    protected bool UpdatePlaneTextureTypes(string? floorType, string? wallType, string? landscapeType)
    {
        if (floorType != _floorType)
        {
            _floorType = floorType;
        }
        else
        {
            floorType = null;
        }

        if (wallType != _wallType)
        {
            _wallType = wallType;
        }
        else
        {
            wallType = null;
        }

        if (landscapeType != _landscapeType)
        {
            _landscapeType = landscapeType;
        }
        else
        {
            landscapeType = null;
        }

        if (floorType == null && wallType == null && landscapeType == null)
        {
            return false;
        }

        foreach (RoomPlane plane in _planes)
        {
            plane.Id = plane.Type switch
            {
                RoomPlane.TYPE_FLOOR when floorType != null => floorType,
                RoomPlane.TYPE_WALL when wallType != null => wallType,
                RoomPlane.TYPE_LANDSCAPE when landscapeType != null => landscapeType,
            };
        }

        return true;
    }

    protected bool UpdatePlanes(IRoomGeometry geometry, bool geometryChanged, int timeStamp)
    {
        IRoomObject? obj = Object;

        if (obj == null || geometry == null)
        {
            return false;
        }

        _renderCount++;

        if (geometryChanged)
        {
            _visiblePlanes = [];
            _visiblePlaneSpriteNumbers = [];
        }

        List<RoomPlane> planes = _visiblePlanes.Count > 0 ? _visiblePlanes : _planes;
        bool hadVisibleBefore = _visiblePlanes.Count > 0;
        bool dirty = false;

        for (int i = 0; i < planes.Count; i++)
        {
            int spriteIndex = hadVisibleBefore ? _visiblePlaneSpriteNumbers[i] : i;

            IRoomObjectSprite? sprite = GetSprite(spriteIndex);

            if (sprite == null)
            {
                continue;
            }

            RoomPlane plane = planes[i];
            sprite.PlaneId = plane.UniqueId;

            if (plane.Update(geometry, timeStamp))
            {
                if (plane.Visible)
                {
                    double depth = plane.RelativeDepth + FloorRelativeDepth + (spriteIndex / 1000.0);

                    if (plane.Type != RoomPlane.TYPE_FLOOR)
                    {
                        depth = plane.RelativeDepth + WallRelativeDepth + (spriteIndex / 1000.0);

                        if (plane.LeftSide.Length < 1 || plane.RightSide.Length < 1)
                        {
                            depth += ROOM_DEPTH_OFFSET * 0.5;
                        }
                    }

                    string assetName = "plane " + spriteIndex + " " + geometry.Scale;
                    UpdateSprite(sprite, plane, assetName, depth);
                }

                dirty = true;
            }

            bool shouldBeVisible = plane.Visible && _typeVisibilities[plane.Type];

            if (sprite.Visible != shouldBeVisible)
            {
                sprite.Visible = shouldBeVisible;
                dirty = true;
            }

            if (sprite.Visible && !hadVisibleBefore)
            {
                _visiblePlanes.Add(plane);
                _visiblePlaneSpriteNumbers.Add(i);
            }
        }

        return dirty;
    }

    protected void UpdatePlaneMasks(string? maskXmlStr)
    {
        if (string.IsNullOrEmpty(maskXmlStr) || _maskParser == null)
        {
            return;
        }

        XElement maskXml = XElement.Parse(maskXmlStr);
        _maskParser.Initialize(maskXml);

        List<int> landscapePlaneIndices = [];
        List<int> visibleLandscapeIndices = [];
        bool visibilityChanged = false;

        for (int i = 0; i < _planes.Count; i++)
        {
            RoomPlane plane = _planes[i];
            plane.ResetBitmapMasks();

            if (plane.Type == RoomPlane.TYPE_LANDSCAPE)
            {
                landscapePlaneIndices.Add(i);
            }
        }

        for (int i = 0; i < _maskParser.MaskCount; i++)
        {
            string? type = _maskParser.GetMaskType(i);
            IVector3d? location = _maskParser.GetMaskLocation(i);
            string? category = _maskParser.GetMaskCategory(i);

            if (location == null)
            {
                continue;
            }

            for (int j = 0; j < _planes.Count; j++)
            {
                RoomPlane plane = _planes[j];

                if (plane.Type != RoomPlane.TYPE_WALL && plane.Type != RoomPlane.TYPE_LANDSCAPE)
                {
                    continue;
                }

                if (plane.Location == null || plane.Normal == null)
                {
                    continue;
                }

                IVector3d diff = Vector3d.Dif(location, plane.Location);
                double normalDist = Math.Abs(Vector3d.ScalarProjection(diff, plane.Normal));

                if (normalDist >= 0.01)
                {
                    continue;
                }

                if (plane.LeftSide == null || plane.RightSide == null)
                {
                    continue;
                }

                double leftProj = Vector3d.ScalarProjection(diff, plane.LeftSide);
                double rightProj = Vector3d.ScalarProjection(diff, plane.RightSide);

                switch (plane.Type)
                {
                    case RoomPlane.TYPE_WALL:
                    case RoomPlane.TYPE_LANDSCAPE when category == "hole":
                        plane.AddBitmapMask(type!, leftProj, rightProj);

                        break;
                    case RoomPlane.TYPE_LANDSCAPE:
                        {
                            if (!plane.CanBeVisible)
                            {
                                visibilityChanged = true;
                            }

                            plane.CanBeVisible = true;
                            visibleLandscapeIndices.Add(j);

                            break;
                        }
                }
            }
        }

        foreach (int idx in landscapePlaneIndices.Where(idx => !visibleLandscapeIndices.Contains(idx)))
        {
            _planes[idx].CanBeVisible = false;
            visibilityChanged = true;
        }

        if (!visibilityChanged)
        {
            return;
        }

        _visiblePlanes = [];
        _visiblePlaneSpriteNumbers = [];
    }

    private void ResetRoomPlanes()
    {
        if (_planes == null)
        {
            return;
        }

        foreach (RoomPlane plane in _planes)
        {
            plane.Dispose();
        }

        _planes = [];
        _planeMap = [];
        _planesInitialized = false;
        _renderCount++;

        Reset();
    }

    private void CreatePlanesAndSprites(int startIndex = 0)
    {
        IRoomObject? obj = Object;

        if (obj == null || _planeParser == null || _visualizationData == null)
        {
            return;
        }

        double landscapeWidth = GetLandscapeWidth();
        double landscapeHeight = GetLandscapeHeight();
        double landscapeOffset = 0;
        int randomSeed = (int)obj.Model.GetNumber("room_random_seed");

        for (int i = startIndex; i < _planeParser.PlaneCount; i++)
        {
            _planeMap[i] = -1;

            IVector3d? location = _planeParser.GetPlaneLocation(i);
            IVector3d? leftSide = _planeParser.GetPlaneLeftSide(i);
            IVector3d? rightSide = _planeParser.GetPlaneRightSide(i);
            List<IVector3d>? secondaryNormals = _planeParser.GetPlaneSecondaryNormals(i);
            int planeType = _planeParser.GetPlaneType(i);

            if (location == null || leftSide == null || rightSide == null)
            {
                return;
            }

            IVector3d normal = Vector3d.CrossProduct(leftSide, rightSide);
            randomSeed = (randomSeed * 7613) + 517;

            RoomPlane? plane = null;

            switch (planeType)
            {
                // floor in parser
                case 1:
                    {
                        double fx = location.X + leftSide.X + 0.5;
                        double fy = location.Y + rightSide.Y + 0.5;
                        double offsetX = (int)fx - fx;
                        double offsetY = (int)fy - fy;

                        plane = new RoomPlane(obj.Location, location, leftSide, rightSide,
                            RoomPlane.TYPE_FLOOR, true, secondaryNormals, randomSeed, -offsetX, -offsetY);

                        if (normal.Z != 0)
                        {
                            plane.Color = FLOOR_COLOR;
                        }
                        else
                        {
                            plane.Color = normal.X != 0 ? FLOOR_COLOR_RIGHT : FLOOR_COLOR_LEFT;
                        }

                        plane.Rasterizer = _visualizationData.FloorRasterizer;

                        break;
                    }
                // wall in parser
                case 2:
                    {
                        plane = new RoomPlane(obj.Location, location, leftSide, rightSide,
                            RoomPlane.TYPE_WALL, true, secondaryNormals, randomSeed);

                        if (leftSide.Length < 1 || rightSide.Length < 1)
                        {
                            plane.HasTexture = false;
                        }

                        if (normal.X == 0 && normal.Y == 0)
                        {
                            plane.Color = WALL_COLOR_BOTTOM;
                        }
                        else
                        {
                            plane.Color = normal.Y switch
                            {
                                > 0 => WALL_COLOR_TOP,
                                0 => WALL_COLOR_SIDE,
                                _ => WALL_COLOR_BOTTOM,
                            };
                        }

                        plane.Rasterizer = _visualizationData.WallRasterizer;

                        break;
                    }
                // landscape in parser
                case 3:
                    {
                        plane = new RoomPlane(obj.Location, location, leftSide, rightSide,
                            RoomPlane.TYPE_LANDSCAPE, true, secondaryNormals, randomSeed,
                            landscapeOffset, 0, landscapeWidth, landscapeHeight)
                        {
                            Color = normal.Y switch
                            {
                                > 0 => LANDSCAPE_COLOR_TOP,
                                0 => LANDSCAPE_COLOR_SIDE,
                                _ => LANDSCAPE_COLOR_BOTTOM,
                            },
                            Rasterizer = _visualizationData.LandscapeRasterizer,
                        };

                        landscapeOffset += leftSide.Length;
                        break;
                    }
                // wall ad in parser
                case 4:
                    {
                        plane = new RoomPlane(obj.Location, location, leftSide, rightSide,
                            RoomPlane.TYPE_WALL, true, secondaryNormals, randomSeed);

                        if (leftSide.Length < 1 || rightSide.Length < 1)
                        {
                            plane.HasTexture = false;
                        }

                        if (normal.X == 0 && normal.Y == 0)
                        {
                            plane.Color = WALL_COLOR_BOTTOM;
                        }
                        else
                        {
                            plane.Color = normal.Y switch
                            {
                                > 0 => WALL_COLOR_TOP,
                                0 => WALL_COLOR_SIDE,
                                _ => WALL_COLOR_BOTTOM,
                            };
                        }

                        plane.Rasterizer = _visualizationData.WallAdRasterizer;

                        break;
                    }
            }

            if (plane == null)
            {
                continue;
            }

            plane.MaskManager = _visualizationData.MaskManager;

            for (int m = 0; m < _planeParser.GetPlaneMaskCount(i); m++)
            {
                double leftLoc = _planeParser.GetPlaneMaskLeftSideLoc(i, m);
                double rightLoc = _planeParser.GetPlaneMaskRightSideLoc(i, m);
                double leftLen = _planeParser.GetPlaneMaskLeftSideLength(i, m);
                double rightLen = _planeParser.GetPlaneMaskRightSideLength(i, m);

                plane.AddRectangleMask(leftLoc, rightLoc, leftLen, rightLen);
            }

            _planeMap[i] = _planes.Count;

            _planes.Add(plane);
        }

        _planesInitialized = true;

        DefineSprites(startIndex);
    }

    private double GetLandscapeWidth()
    {
        if (_planeParser == null)
        {
            return 0;
        }

        double width = 0;

        for (int i = 0; i < _planeParser.PlaneCount; i++)
        {
            if (_planeParser.GetPlaneType(i) != 3)
            {
                continue;
            }

            IVector3d? leftSide = _planeParser.GetPlaneLeftSide(i);

            if (leftSide != null)
            {
                width += leftSide.Length;
            }
        }

        return width;
    }

    private double GetLandscapeHeight()
    {
        if (_planeParser == null)
        {
            return 0;
        }

        double height = 0;

        for (int i = 0; i < _planeParser.PlaneCount; i++)
        {
            if (_planeParser.GetPlaneType(i) != 3)
            {
                continue;
            }

            IVector3d? rightSide = _planeParser.GetPlaneRightSide(i);

            if (rightSide != null && rightSide.Length > height)
            {
                height = rightSide.Length;
            }
        }

        if (height > 5)
        {
            height = 5;
        }

        return height;
    }

    private bool UpdateGeometry(IRoomGeometry geometry)
    {
        bool changed = false;

        if (geometry.UpdateId == _geometryUpdateId)
        {
            return changed;
        }
        _geometryUpdateId = geometry.UpdateId;

        IVector3d? dir = geometry.Direction;

        if (dir == null || (dir.X == _dirX && dir.Y == _dirY && dir.Z == _dirZ && geometry.Scale == _cachedScale))
        {
            return changed;
        }

        _dirX = dir.X;
        _dirY = dir.Y;
        _dirZ = dir.Z;
        _cachedScale = geometry.Scale;
        changed = true;

        return changed;
    }

    private bool UpdateMasksAndColors(IRoomObjectModel model)
    {
        bool dirty = false;

        if (_previousModelUpdateId == model.UpdateId)
        {
            return dirty;
        }

        string? maskXmlStr = model.GetString("room_plane_mask_xml");

        if (maskXmlStr != _maskXml)
        {
            UpdatePlaneMasks(maskXmlStr);
            _maskXml = maskXmlStr;
            dirty = true;
        }

        uint bgColor = (uint)model.GetNumber("room_background_color");

        if (bgColor == _backgroundColor)
        {
            return dirty;
        }

        _backgroundColor = bgColor;
        _backgroundBlue = (int)(_backgroundColor & 0xFF);
        _backgroundGreen = (int)((_backgroundColor >> 8) & 0xFF);
        _backgroundRed = (int)((_backgroundColor >> 16) & 0xFF);
        dirty = true;

        return dirty;
    }

    private bool UpdatePlaneTexturesAndVisibilities(IRoomObjectModel model)
    {
        if (_previousModelUpdateId == model.UpdateId)
        {
            return false;
        }

        string? wallType = model.GetString("room_wall_type");
        string? floorType = model.GetString("room_floor_type");
        string? landscapeType = model.GetString("room_landscape_type");

        UpdatePlaneTextureTypes(floorType, wallType, landscapeType);

        bool floorVis = model.GetNumber("room_floor_visibility") != 0;
        bool wallVis = model.GetNumber("room_wall_visibility") != 0;
        bool landscapeVis = model.GetNumber("room_landscape_visibility") != 0;

        UpdatePlaneTypeVisibilities(floorVis, wallVis, landscapeVis);
        return true;

    }

    private bool UpdatePlaneThicknesses(IRoomObjectModel model)
    {
        if (_previousModelUpdateId == model.UpdateId)
        {
            return false;
        }

        double floor = model.GetNumber("room_floor_thickness");
        double wall = model.GetNumber("room_wall_thickness");

        if (double.IsNaN(floor) || double.IsNaN(wall) ||
            (floor == _floorThickness && wall == _wallThickness))
        {
            return false;
        }

        _floorThickness = floor;
        _wallThickness = wall;

        ResetRoomPlanes();

        return true;

    }

    private bool UpdateFloorHoles(IRoomObjectModel model)
    {
        if (_previousModelUpdateId == model.UpdateId)
        {
            return false;
        }

        double holeTime = model.GetNumber("room_floor_hole_update_time");

        if (double.IsNaN(holeTime) || holeTime == _floorHoleUpdateTime)
        {
            return false;
        }

        _floorHoleUpdateTime = holeTime;

        ResetRoomPlanes();

        return true;

    }

    private void UpdatePlaneTypeVisibilities(bool floor, bool wall, bool landscape)
    {
        if (floor == _typeVisibilities[RoomPlane.TYPE_FLOOR] &&
            wall == _typeVisibilities[RoomPlane.TYPE_WALL] &&
            landscape == _typeVisibilities[RoomPlane.TYPE_LANDSCAPE])
        {
            return;
        }

        _typeVisibilities[RoomPlane.TYPE_FLOOR] = floor;
        _typeVisibilities[RoomPlane.TYPE_WALL] = wall;
        _typeVisibilities[RoomPlane.TYPE_LANDSCAPE] = landscape;
        _visiblePlanes = [];
        _visiblePlaneSpriteNumbers = [];
    }

    private void UpdateSprite(IRoomObjectSprite sprite, RoomPlane plane, string assetName, double depth)
    {
        Vector2 offset = plane.Offset;
        sprite.OffsetX = -(int)offset.X;
        sprite.OffsetY = -(int)offset.Y;
        sprite.RelativeDepth = depth;
        sprite.Color = (int)plane.Color;

        Image? bitmap = plane.CopyBitmapData(sprite.Asset);

        if (bitmap == null)
        {
            bitmap = plane.BitmapData;
        }

        sprite.Asset = bitmap;
        sprite.AssetName = assetName + "_" + _renderCount;
    }
}
