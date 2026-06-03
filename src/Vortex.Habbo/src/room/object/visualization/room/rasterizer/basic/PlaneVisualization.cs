using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;
using Vortex.Room.Utils;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneVisualization
public class PlaneVisualization
{
    private List<object?>? _layers;
    private Image? _cachedBitmap;
    private Vector3d? _cachedBitmapNormal;
    private bool _isCached;

    public PlaneVisualization(double size, int layerCount, IRoomGeometry? geometry)
    {
        _layers = [];

        if (layerCount < 0)
        {
            layerCount = 0;
        }

        for (int i = 0; i < layerCount; i++)
        {
            _layers.Add(null);
        }

        Geometry = geometry;
        _cachedBitmapNormal = new Vector3d();
    }

    public IRoomGeometry? Geometry { get; private set; }

    public bool HasAnimationLayers { get; private set; }

    public void Dispose()
    {
        if (_layers != null)
        {
            foreach (object? layer in _layers)
            {
                if (layer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _layers = null;
        }

        Geometry = null;
        _cachedBitmap = null;
        _cachedBitmapNormal = null;
    }

    public void ClearCache()
    {
        if (!_isCached)
        {
            return;
        }

        _cachedBitmap = null;

        if (_cachedBitmapNormal != null)
        {
            _cachedBitmapNormal.Assign(new Vector3d());
        }

        if (_layers != null)
        {
            foreach (object? layer in _layers)
            {
                if (layer is PlaneVisualizationLayer vizLayer)
                {
                    vizLayer.ClearCache();
                }
                else if (layer is PlaneVisualizationAnimationLayer animLayer)
                {
                    animLayer.ClearCache();
                }
            }
        }

        _isCached = false;
    }

    public bool SetLayer(int index, PlaneMaterial? material, uint color, int align, int offset = 0)
    {
        if (_layers == null || index < 0 || index >= _layers.Count)
        {
            return false;
        }

        if (_layers[index] is IDisposable existing)
        {
            existing.Dispose();
        }

        _layers[index] = new PlaneVisualizationLayer(material, color, align, offset);
        return true;
    }

    public bool SetAnimationLayer(int index, System.Xml.Linq.XElement? xml,
        Vortex.Room.Object.Visualization.Utils.IGraphicAssetCollection? assetCollection)
    {
        if (_layers == null || index < 0 || index >= _layers.Count)
        {
            return false;
        }

        if (_layers[index] is IDisposable existing)
        {
            existing.Dispose();
        }

        _layers[index] = new PlaneVisualizationAnimationLayer(xml, assetCollection);
        HasAnimationLayers = true;
        return true;
    }

    public List<object?>? GetLayers()
    {
        return _layers;
    }

    public Image? Render(
        Image? canvas, int width, int height, IVector3d normal, bool useTexture,
        int offsetX = 0, int offsetY = 0,
        int animOffsetX = 0, int animOffsetY = 0,
        double animScrollX = 0, double animScrollY = 0,
        int timeStamp = 0)
    {
        if (width < 1)
        {
            width = 1;
        }

        if (height < 1)
        {
            height = 1;
        }

        if (canvas != null && (canvas.GetWidth() != width || canvas.GetHeight() != height))
        {
            canvas = null;
        }

        // Check cached bitmap
        if (_cachedBitmap != null)
        {
            if (_cachedBitmap.GetWidth() == width && _cachedBitmap.GetHeight() == height &&
                Vector3d.IsEqual(_cachedBitmapNormal, normal))
            {
                if (!HasAnimationLayers)
                {
                    if (canvas != null)
                    {
                        canvas.BlitRect(_cachedBitmap, new Rect2I(0, 0, width, height), Vector2I.Zero);
                        return canvas;
                    }

                    return _cachedBitmap;
                }
            }
            else
            {
                _cachedBitmap = null;
            }
        }

        _isCached = true;

        if (_cachedBitmap == null)
        {
            _cachedBitmap = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        }
        else
        {
            _cachedBitmap.Fill(new Color(1f, 1f, 1f, 0f));
        }

        canvas ??= _cachedBitmap;

        _cachedBitmapNormal?.Assign(normal);

        // Render layers
        if (_layers != null)
        {
            foreach (object? layer in _layers)
            {
                if (layer is PlaneVisualizationLayer vizLayer)
                {
                    vizLayer.Render(canvas, width, height, normal, useTexture, offsetX, offsetY);
                }
                else if (layer is PlaneVisualizationAnimationLayer animLayer)
                {
                    animLayer.Render(canvas, width, height, normal, offsetX, offsetY,
                        animOffsetX, animOffsetY, animScrollX, animScrollY, timeStamp);
                }
            }
        }

        // Cache the result
        if (canvas != _cachedBitmap)
        {
            _cachedBitmap.BlitRect(canvas, new Rect2I(0, 0, width, height), Vector2I.Zero);
            return canvas;
        }

        return _cachedBitmap;
    }
}
