using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneRasterizer
public class PlaneRasterizer : IPlaneRasterizer
{
    protected const string DEFAULT_TYPE = "default";

    private Dictionary<string, PlaneMaterial>? _materials = [];
    private Dictionary<string, PlaneTexture>? _textures = [];
    private Dictionary<string, Plane>? _planes = [];
    private Dictionary<string, RoomGeometry>? _geometries = [];

    protected XElement? Data { get; private set; }

    protected IGraphicAssetCollection? AssetCollection { get; private set; }

    public virtual bool InitializeDimensions(int width, int height)
    {
        return true;
    }

    public virtual void Dispose()
    {
        if (_planes != null)
        {
            foreach (Plane plane in _planes.Values)
            {
                plane.Dispose();
            }

            _planes = null;
        }

        if (_materials != null)
        {
            ResetMaterials();
            _materials = null;
        }

        if (_textures != null)
        {
            ResetTextures();
            _textures = null;
        }

        if (_geometries != null)
        {
            foreach (RoomGeometry geo in _geometries.Values)
            {
                geo.Dispose();
            }

            _geometries = null;
        }

        Data = null;
        AssetCollection = null;
    }

    public virtual void ClearCache()
    {
        if (_planes != null)
        {
            foreach (Plane plane in _planes.Values)
            {
                plane.ClearCache();
            }
        }

        if (_materials != null)
        {
            foreach (PlaneMaterial material in _materials.Values)
            {
                material.ClearCache();
            }
        }
    }

    public void Initialize(XElement? data)
    {
        Data = data;
    }

    public void Reinitialize()
    {
        ResetTextures();
        ResetMaterials();
        InitializeAll();
    }

    public void InitializeAssetCollection(IGraphicAssetCollection? assetCollection)
    {
        if (Data == null)
        {
            return;
        }

        AssetCollection = assetCollection;

        InitializeAll();
    }

    public virtual PlaneBitmapData? Render(
        Image? canvas, string type,
        double width, double height, double scale,
        IVector3d normal, bool useTexture,
        double offsetX = 0, double offsetY = 0,
        double topAlignOffset = 0, double bottomAlignOffset = 0,
        int timeStamp = 0)
    {
        return null;
    }

    public virtual string GetTextureIdentifier(double scale, IVector3d? normal)
    {
        return scale.ToString(CultureInfo.CurrentCulture);
    }

    public object[]? GetLayers(string type)
    {
        Plane? plane = GetPlane(type) ?? GetPlane(DEFAULT_TYPE);

        List<object?>? layers = plane?.GetLayers();

        return layers?.ToArray();
    }

    protected PlaneTexture? GetTexture(string id)
    {
        if (_textures == null)
        {
            return null;
        }

        _textures.TryGetValue(id, out PlaneTexture? texture);
        return texture;
    }

    protected PlaneMaterial? GetMaterial(string id)
    {
        if (_materials == null)
        {
            return null;
        }

        _materials.TryGetValue(id, out PlaneMaterial? material);
        return material;
    }

    protected Plane? GetPlane(string id)
    {
        if (_planes == null)
        {
            return null;
        }

        _planes.TryGetValue(id, out Plane? plane);
        return plane;
    }

    protected bool AddPlane(string id, Plane? plane)
    {
        if (plane == null || _planes == null)
        {
            return false;
        }

        return _planes.TryAdd(id, plane);
    }

    protected virtual void InitializePlanes()
    {
    }

    protected IRoomGeometry GetGeometry(int size, double horizontalAngle, double verticalAngle)
    {
        horizontalAngle = Math.Abs(horizontalAngle);

        if (horizontalAngle > 90)
        {
            horizontalAngle = 90;
        }

        verticalAngle = Math.Abs(verticalAngle);

        if (verticalAngle > 90)
        {
            verticalAngle = 90;
        }

        string key = size + "_" + Math.Round(horizontalAngle) + "_" + Math.Round(verticalAngle);

        _geometries ??= [];

        if (_geometries.TryGetValue(key, out RoomGeometry? geometry))
        {
            return geometry;
        }

        geometry = new RoomGeometry(size, new Vector3d(horizontalAngle, verticalAngle), new Vector3d(-10, 0, 0));
        _geometries[key] = geometry;

        return geometry;
    }

    protected void ParseVisualizations(Plane plane, IEnumerable<XElement>? visualizations)
    {
        if (visualizations == null)
        {
            return;
        }

        foreach (XElement vizElement in visualizations)
        {
            XAttribute? sizeAttr = vizElement.Attribute("size");

            if (sizeAttr == null)
            {
                continue;
            }

            int size = (int)sizeAttr;

            string? horizontalAngleStr = (string?)vizElement.Attribute("horizontalAngle");
            string? verticalAngleStr = (string?)vizElement.Attribute("verticalAngle");

            double horizontalAngle = 45;

            if (!string.IsNullOrEmpty(horizontalAngleStr))
            {
                horizontalAngle = double.Parse(horizontalAngleStr);
            }

            double verticalAngle = 30;

            if (!string.IsNullOrEmpty(verticalAngleStr))
            {
                verticalAngle = double.Parse(verticalAngleStr);
            }

            IEnumerable<XElement> layerElements = vizElement.Elements("visualizationLayer");
            int layerCount = layerElements.Count();

            PlaneVisualization? viz = plane.CreatePlaneVisualization(size, layerCount, GetGeometry(size, horizontalAngle, verticalAngle));

            if (viz == null)
            {
                continue;
            }

            int layerIndex = 0;

            foreach (XElement layerElement in vizElement.Elements("visualizationLayer"))
            {
                PlaneMaterial? material = null;
                int align = PlaneVisualizationLayer.ALIGN_TOP;

                string? materialId = (string?)layerElement.Attribute("materialId");

                if (!string.IsNullOrEmpty(materialId))
                {
                    material = GetMaterial(materialId);
                }

                string? offsetStr = (string?)layerElement.Attribute("offset");
                int offset = 0;

                if (!string.IsNullOrEmpty(offsetStr))
                {
                    offset = int.Parse(offsetStr);
                }

                string? colorStr = (string?)layerElement.Attribute("color");
                uint color = 0xFFFFFF;

                if (!string.IsNullOrEmpty(colorStr))
                {
                    if (colorStr.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
                    {
                        color = uint.Parse(colorStr.AsSpan(2), System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        color = uint.Parse(colorStr);
                    }
                }

                string? alignStr = (string?)layerElement.Attribute("align");

                align = alignStr switch
                {
                    "bottom" => PlaneVisualizationLayer.ALIGN_BOTTOM,
                    "top" => PlaneVisualizationLayer.ALIGN_TOP,
                    _ => align,
                };

                viz.SetLayer(layerIndex, material, color, align, offset);
                layerIndex++;
            }
        }
    }

    private void ResetMaterials()
    {
        if (_materials == null)
        {
            return;
        }

        foreach (PlaneMaterial material in _materials.Values)
        {
            material.Dispose();
        }

        _materials.Clear();
    }

    private void ResetTextures()
    {
        if (_textures == null)
        {
            return;
        }

        foreach (PlaneTexture texture in _textures.Values)
        {
            texture.Dispose();
        }

        _textures.Clear();
    }

    private void InitializeAll()
    {
        if (Data == null)
        {
            return;
        }

        InitializeTexturesAndMaterials();
        InitializePlanes();
    }

    private void InitializeTexturesAndMaterials()
    {
        XElement? texturesElement = Data?.Element("textures");

        if (texturesElement != null)
        {
            ParseTextures(texturesElement, AssetCollection);
        }

        XElement? materialsElement = Data?.Element("materials");

        if (materialsElement != null)
        {
            ParsePlaneMaterials(materialsElement);
        }
    }

    private void ParseTextures(XElement texturesElement, IGraphicAssetCollection? assetCollection)
    {
        if (assetCollection == null)
        {
            return;
        }

        foreach (XElement textureElement in texturesElement.Elements("texture"))
        {
            string? id = (string?)textureElement.Attribute("id");

            if (string.IsNullOrEmpty(id) || _textures == null)
            {
                continue;
            }

            if (_textures.ContainsKey(id))
            {
                continue;
            }

            PlaneTexture texture = new();

            foreach (XElement bitmapElement in textureElement.Elements("bitmap"))
            {
                string? assetName = (string?)bitmapElement.Attribute("assetName");

                if (string.IsNullOrEmpty(assetName))
                {
                    continue;
                }

                double normalMinX = (double?)bitmapElement.Attribute("normalMinX") ?? -1;
                double normalMaxX = (double?)bitmapElement.Attribute("normalMaxX") ?? 1;
                double normalMinY = (double?)bitmapElement.Attribute("normalMinY") ?? -1;
                double normalMaxY = (double?)bitmapElement.Attribute("normalMaxY") ?? 1;

                IGraphicAsset? asset = assetCollection.GetAsset(assetName);

                if (asset?.Asset is not BitmapDataAsset bitmapAsset)
                {
                    continue;
                }

                if (bitmapAsset.Content is not Image bitmapData)
                {
                    continue;
                }

                Image textureImage;

                if (asset.FlipH)
                {
                    textureImage = BitmapDataUtil.GetFlipHBitmapData(bitmapData) ?? (Image)bitmapData.Duplicate();
                }
                else
                {
                    textureImage = (Image)bitmapData.Duplicate();
                }

                texture.AddBitmap(textureImage, normalMinX, normalMaxX, normalMinY, normalMaxY, assetName);
            }

            _textures[id] = texture;
        }
    }

    private void ParsePlaneMaterials(XElement materialsElement)
    {
        foreach (XElement materialElement in materialsElement.Elements("material"))
        {
            string? id = (string?)materialElement.Attribute("id");

            if (string.IsNullOrEmpty(id) || _materials == null)
            {
                continue;
            }

            PlaneMaterial material = new();

            foreach (XElement matrixElement in materialElement.Elements("materialCellMatrix"))
            {
                string? repeatModeStr = (string?)matrixElement.Attribute("repeatMode");
                string? alignStr = (string?)matrixElement.Attribute("align");

                int repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_ALL;

                switch (repeatModeStr)
                {
                    case "borders":
                        repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_BORDERS;
                        break;
                    case "center":
                        repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_CENTER;
                        break;
                    case "first":
                        repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_FIRST;
                        break;
                    case "last":
                        repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_LAST;
                        break;
                    case "random":
                        repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_RANDOM;
                        break;
                }

                int matAlign = PlaneMaterialCellMatrix.ALIGN_TOP;

                switch (alignStr)
                {
                    case "bottom":
                        matAlign = PlaneMaterialCellMatrix.ALIGN_BOTTOM;
                        break;
                }

                double normalMinX = (double?)matrixElement.Attribute("normalMinX") ?? -1;
                double normalMaxX = (double?)matrixElement.Attribute("normalMaxX") ?? 1;
                double normalMinY = (double?)matrixElement.Attribute("normalMinY") ?? -1;
                double normalMaxY = (double?)matrixElement.Attribute("normalMaxY") ?? 1;

                IEnumerable<XElement> columnElements = matrixElement.Elements("materialCellColumn");
                int columnCount = 0;

                foreach (XElement _ in columnElements)
                {
                    columnCount++;
                }

                PlaneMaterialCellMatrix matrix = material.AddMaterialCellMatrix(
                    columnCount, repeatMode, matAlign,
                    normalMinX, normalMaxX, normalMinY, normalMaxY);

                int colIndex = 0;

                foreach (XElement columnElement in matrixElement.Elements("materialCellColumn"))
                {
                    ParsePlaneMaterialCellColumn(columnElement, matrix, colIndex);
                    colIndex++;
                }
            }

            _materials[id] = material;
        }
    }

    private void ParsePlaneMaterialCellColumn(XElement columnElement, PlaneMaterialCellMatrix matrix, int index)
    {
        string? repeatModeStr = (string?)columnElement.Attribute("repeatMode");
        int width = (int?)columnElement.Attribute("width") ?? 1;

        int repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_ALL;

        switch (repeatModeStr)
        {
            case "borders":
                repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_BORDERS;
                break;
            case "center":
                repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_CENTER;
                break;
            case "first":
                repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_FIRST;
                break;
            case "last":
                repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_LAST;
                break;
            case "none":
                repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_NONE;
                break;
        }

        List<PlaneMaterialCell>? cells = ParsePlaneMaterialCells(columnElement);
        matrix.CreateColumn(index, width, cells, repeatMode);
    }

    private List<PlaneMaterialCell>? ParsePlaneMaterialCells(XElement columnElement)
    {
        List<PlaneMaterialCell> cells = [];

        foreach (XElement cellElement in columnElement.Elements("materialCell"))
        {
            string? textureId = (string?)cellElement.Attribute("textureId");
            List<IGraphicAsset>? extraAssets = null;
            List<Vector2I>? offsets = null;
            int limitMax = 0;

            XElement? extraItemDataElement = cellElement.Element("extraItemData");

            if (extraItemDataElement != null)
            {
                XElement? extraItemTypesElement = extraItemDataElement.Element("extraItemTypes");
                XElement? offsetsElement = extraItemDataElement.Element("offsets");

                if (extraItemTypesElement != null && offsetsElement != null)
                {
                    List<string> assetNames = ParseExtraItemTypes(extraItemTypesElement);
                    offsets = ParseExtraItemOffsets(offsetsElement);
                    limitMax = offsets.Count;

                    string? limitMaxStr = (string?)extraItemDataElement.Attribute("limitMax");

                    if (!string.IsNullOrEmpty(limitMaxStr))
                    {
                        limitMax = int.Parse(limitMaxStr);
                    }

                    if (AssetCollection != null)
                    {
                        extraAssets = [];

                        foreach (string name in assetNames)
                        {
                            IGraphicAsset? asset = AssetCollection.GetAsset(name);

                            if (asset != null)
                            {
                                extraAssets.Add(asset);
                            }
                        }
                    }
                }
            }

            PlaneTexture? texture = textureId != null ? GetTexture(textureId) : null;
            PlaneMaterialCell cell = new(texture, extraAssets, offsets, limitMax);
            cells.Add(cell);
        }

        return cells.Count == 0 ? null : cells;
    }

    private static List<string> ParseExtraItemTypes(XElement typesElement)
    {
        List<string> result = [];

        foreach (XElement typeElement in typesElement.Elements("extraItemType"))
        {
            string? assetName = (string?)typeElement.Attribute("assetName");

            if (!string.IsNullOrEmpty(assetName))
            {
                result.Add(assetName);
            }
        }

        return result;
    }

    private static List<Vector2I> ParseExtraItemOffsets(XElement offsetsElement)
    {
        List<Vector2I> result = [];

        foreach (XElement offsetElement in offsetsElement.Elements("offset"))
        {
            int? x = (int?)offsetElement.Attribute("x");
            int? y = (int?)offsetElement.Attribute("y");

            if (x.HasValue && y.HasValue)
            {
                result.Add(new Vector2I(x.Value, y.Value));
            }
        }

        return result;
    }
}
