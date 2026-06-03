using System;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurniturePlane
public class FurniturePlane
{
    private int _geometryUpdateId = -1;
    private double _cachedDirX;
    private double _cachedDirY;
    private double _cachedDirZ;
    private double _cachedScale;
    private Vector3d _origin;
    private Vector3d _location;
    private Vector3d _leftSide;
    private Vector3d _rightSide;
    private Vector3d _originalLeftSide;
    private Vector3d _originalRightSide;
    private Dictionary<string, Image>? _textureCache;
    private bool _isRotated;
    private Vector3d _cornerA;
    private Vector3d _cornerB;
    private Vector3d _cornerC;
    private Vector3d _cornerD;
    private double _width;
    private double _height;
    private Vector3d _normal;
    private Image? _bitmapData;
    private Vector2 _offset;

    public FurniturePlane(IVector3d location, IVector3d leftSide, IVector3d rightSide)
    {
        _origin = new Vector3d();
        _location = new Vector3d();
        _location.Assign(location);
        _leftSide = new Vector3d();
        _leftSide.Assign(leftSide);
        _rightSide = new Vector3d();
        _rightSide.Assign(rightSide);
        _originalLeftSide = new Vector3d();
        _originalLeftSide.Assign(leftSide);
        _originalRightSide = new Vector3d();
        _originalRightSide.Assign(rightSide);
        _normal = Vector3d.CrossProduct(_leftSide, _rightSide) ?? new Vector3d();

        if (_normal.Length > 0)
        {
            _normal.Mul(1.0 / _normal.Length);
        }

        _offset = Vector2.Zero;
        _cornerA = new Vector3d();
        _cornerB = new Vector3d();
        _cornerC = new Vector3d();
        _cornerD = new Vector3d();
        _textureCache = new Dictionary<string, Image>();
    }

    public IVector3d Normal => _normal;

    public Image? BitmapData
    {
        get
        {
            if (Visible && _bitmapData != null)
            {
                return (Image)_bitmapData.Duplicate();
            }

            return null;
        }
    }

    public Vector2 Offset => _offset;

    public double RelativeDepth { get; private set; }

    public uint Color { get; set; }

    public bool Visible { get; private set; } = true;

    public IVector3d LeftSide => _leftSide;

    public IVector3d RightSide => _rightSide;

    public IVector3d Location => _location;

    public void Dispose()
    {
        _bitmapData = null;

        if (_textureCache != null)
        {
            _textureCache.Clear();
            _textureCache = null;
        }

        _origin = null!;
        _location = null!;
        _leftSide = null!;
        _rightSide = null!;
        _originalLeftSide = null!;
        _originalRightSide = null!;
        _normal = null!;
        _cornerA = null!;
        _cornerB = null!;
        _cornerC = null!;
        _cornerD = null!;
    }

    public void SetRotation(bool rotated)
    {
        if (rotated == _isRotated)
        {
            return;
        }

        if (!rotated)
        {
            _leftSide.Assign(_originalLeftSide);
            _rightSide.Assign(_originalRightSide);
        }
        else
        {
            _leftSide.Assign(_originalLeftSide);
            _leftSide.Mul(_originalRightSide.Length / _originalLeftSide.Length);
            _rightSide.Assign(_originalRightSide);
            _rightSide.Mul(_originalLeftSide.Length / _originalRightSide.Length);
        }

        _geometryUpdateId = -1;
        _cachedDirX -= 1;
        _isRotated = rotated;
        ResetTextureCache();
    }

    public bool Update(IRoomGeometry? geometry, int colorParam)
    {
        if (geometry == null || _leftSide == null || _rightSide == null || _normal == null)
        {
            return false;
        }

        bool changed = false;

        if (geometry.UpdateId != _geometryUpdateId)
        {
            _geometryUpdateId = geometry.UpdateId;
            IVector3d? dir = geometry.Direction;

            if (dir != null && (dir.X != _cachedDirX || dir.Y != _cachedDirY || dir.Z != _cachedDirZ || geometry.Scale != _cachedScale))
            {
                _cachedDirX = dir.X;
                _cachedDirY = dir.Y;
                _cachedDirZ = dir.Z;
                _cachedScale = geometry.Scale;
                changed = true;

                double cosAngle = Vector3d.CosAngle(geometry.DirectionAxis, _normal);

                if (cosAngle > -0.001)
                {
                    if (Visible)
                    {
                        Visible = false;
                        return true;
                    }

                    return false;
                }

                UpdateCorners(geometry);

                IVector3d? originScreen = geometry.GetScreenPosition(_origin);
                double originZ = originScreen?.Z ?? 0;

                RelativeDepth = Math.Max(
                    _cornerA.Z - originZ,
                    Math.Max(
                        _cornerB.Z - originZ,
                        Math.Max(_cornerC.Z - originZ, _cornerD.Z - originZ)));

                Visible = true;
            }
        }

        if (NeedsNewTexture(geometry) || changed)
        {
            int w = (int)_width;
            int h = (int)_height;

            if (_bitmapData == null || w != _bitmapData.GetWidth() || h != _bitmapData.GetHeight())
            {
                _bitmapData = null;

                if (w < 1 || h < 1)
                {
                    return changed;
                }

                _bitmapData = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
            }
            else
            {
                _bitmapData.Fill(new Color(1f, 1f, 1f, 0f));
            }

            Image? texture = GetTexture(geometry, colorParam);

            if (texture != null)
            {
                RenderTexture(texture);
            }

            return true;
        }

        return false;
    }

    private void CacheTexture(string key, Image texture)
    {
        if (_textureCache == null)
        {
            return;
        }

        _textureCache[key] = texture;
    }

    private void ResetTextureCache()
    {
        _textureCache?.Clear();
    }

    private static string GetTextureIdentifier(IRoomGeometry geometry)
    {
        return geometry.Scale.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private bool NeedsNewTexture(IRoomGeometry? geometry)
    {
        if (geometry == null || _textureCache == null)
        {
            return false;
        }

        string key = GetTextureIdentifier(geometry);

        if (_width > 0 && _height > 0)
        {
            if (!_textureCache.ContainsKey(key))
            {
                return true;
            }
        }

        return false;
    }

    private Image? GetTexture(IRoomGeometry? geometry, int colorParam)
    {
        if (geometry == null || _textureCache == null)
        {
            return null;
        }

        string key = GetTextureIdentifier(geometry);

        if (NeedsNewTexture(geometry))
        {
            double texW = _leftSide.Length * geometry.Scale;
            double texH = _rightSide.Length * geometry.Scale;

            if (texW < 1)
            {
                texW = 1;
            }

            if (texH < 1)
            {
                texH = 1;
            }

            if (!_textureCache.TryGetValue(key, out Image? cached))
            {
                float r = ((Color >> 16) & 0xFF) / 255f;
                float g = ((Color >> 8) & 0xFF) / 255f;
                float b = (Color & 0xFF) / 255f;

                cached = Image.CreateEmpty((int)texW, (int)texH, false, Image.Format.Rgba8);
                cached.Fill(new Color(r, g, b, 1f));
                CacheTexture(key, cached);
            }

            return cached;
        }

        _textureCache.TryGetValue(key, out Image? texture);
        return texture;
    }

    private void UpdateCorners(IRoomGeometry geometry)
    {
        IVector3d? screenA = geometry.GetScreenPosition(_location);
        Vector3d? locPlusRight = Vector3d.Sum(_location, _rightSide);
        Vector3d? locPlusLeftRight = Vector3d.Sum(Vector3d.Sum(_location, _leftSide), _rightSide);
        Vector3d? locPlusLeft = Vector3d.Sum(_location, _leftSide);

        if (screenA != null)
        {
            _cornerA.Assign(screenA);
        }

        if (locPlusRight != null)
        {
            IVector3d? screenB = geometry.GetScreenPosition(locPlusRight);

            if (screenB != null)
            {
                _cornerB.Assign(screenB);
            }
        }

        if (locPlusLeftRight != null)
        {
            IVector3d? screenC = geometry.GetScreenPosition(locPlusLeftRight);

            if (screenC != null)
            {
                _cornerC.Assign(screenC);
            }
        }

        if (locPlusLeft != null)
        {
            IVector3d? screenD = geometry.GetScreenPosition(locPlusLeft);

            if (screenD != null)
            {
                _cornerD.Assign(screenD);
            }
        }

        Vector2? screenPoint = geometry.GetScreenPoint(_origin);
        _offset = screenPoint ?? Vector2.Zero;

        _cornerA.X = Math.Round(_cornerA.X);
        _cornerA.Y = Math.Round(_cornerA.Y);
        _cornerB.X = Math.Round(_cornerB.X);
        _cornerB.Y = Math.Round(_cornerB.Y);
        _cornerC.X = Math.Round(_cornerC.X);
        _cornerC.Y = Math.Round(_cornerC.Y);
        _cornerD.X = Math.Round(_cornerD.X);
        _cornerD.Y = Math.Round(_cornerD.Y);
        _offset = new Vector2((float)Math.Round(_offset.X), (float)Math.Round(_offset.Y));

        double minX = Math.Min(_cornerA.X, Math.Min(_cornerB.X, Math.Min(_cornerC.X, _cornerD.X)));
        double maxX = Math.Max(_cornerA.X, Math.Max(_cornerB.X, Math.Max(_cornerC.X, _cornerD.X)));
        double minY = Math.Min(_cornerA.Y, Math.Min(_cornerB.Y, Math.Min(_cornerC.Y, _cornerD.Y)));
        double maxY = Math.Max(_cornerA.Y, Math.Max(_cornerB.Y, Math.Max(_cornerC.Y, _cornerD.Y)));

        maxX -= minX;
        _offset = new Vector2(_offset.X - (float)minX, _offset.Y - (float)minY);
        _cornerA.X -= minX;
        _cornerB.X -= minX;
        _cornerC.X -= minX;
        _cornerD.X -= minX;

        maxY -= minY;
        _cornerA.Y -= minY;
        _cornerB.Y -= minY;
        _cornerC.Y -= minY;
        _cornerD.Y -= minY;

        _width = maxX;
        _height = maxY;
    }

    private void RenderTexture(Image texture)
    {
        if (_cornerA == null || _cornerB == null || _cornerC == null || _cornerD == null || _bitmapData == null)
        {
            return;
        }

        int texW = texture.GetWidth();
        int texH = texture.GetHeight();

        double dxRight = _cornerB.X - _cornerC.X;
        double dyRight = _cornerB.Y - _cornerC.Y;
        double dxLeft = _cornerD.X - _cornerC.X;
        double dyLeft = _cornerD.Y - _cornerC.Y;

        if (Math.Abs(dxRight - texW) <= 1)
        {
            dxRight = texW;
        }

        if (Math.Abs(dyRight - texW) <= 1)
        {
            dyRight = texW;
        }

        if (Math.Abs(dxLeft - texH) <= 1)
        {
            dxLeft = texH;
        }

        if (Math.Abs(dyLeft - texH) <= 1)
        {
            dyLeft = texH;
        }

        double a = dxRight / texW;
        double b = dyRight / texW;
        double c = dxLeft / texH;
        double d = dyLeft / texH;
        double tx = _cornerC.X;
        double ty = _cornerC.Y;

        Draw(texture, a, b, c, d, tx, ty);
    }

    private void Draw(Image source, double a, double b, double c, double d, double tx, double ty)
    {
        if (_bitmapData == null)
        {
            return;
        }

        int srcW = source.GetWidth();
        int srcH = source.GetHeight();
        int dstW = _bitmapData.GetWidth();
        int dstH = _bitmapData.GetHeight();

        if (a == 1 && d == 1 && c == 0 && b != 0 && Math.Abs(b) <= 1)
        {
            DrawSheared(source, b, tx, ty, srcW, srcH, dstW, dstH);
            return;
        }

        DrawAffine(source, a, b, c, d, tx, ty, srcW, srcH, dstW, dstH);
    }

    /// <summary>
    /// Optimized path for vertical shear transforms (a=1, d=1, c=0, |b|&lt;=1).
    /// </summary>
    private void DrawSheared(Image source, double b, double tx, double ty, int srcW, int srcH, int dstW, int dstH)
    {
        int currentX = 0;
        int stripStart = 0;
        double accum = 0;
        int yOffset = 0;
        int tyInt = (int)ty;

        if (b > 0)
        {
            tyInt++;
        }

        int txInt = (int)tx;

        while (currentX < srcW)
        {
            currentX++;
            accum += Math.Abs(b);

            if (accum >= 1)
            {
                int stripW = currentX - stripStart;
                int destX = txInt + stripStart;
                int destY = tyInt + yOffset;

                BlitStrip(source, _bitmapData, stripStart, 0, stripW, srcH, destX, destY, dstW, dstH);

                stripStart = currentX;
                yOffset += b > 0 ? 1 : -1;
                accum = 0;
            }
        }

        if (accum > 0)
        {
            int stripW = currentX - stripStart;
            int destX = txInt + stripStart;
            int destY = tyInt + yOffset;

            BlitStrip(source, _bitmapData, stripStart, 0, stripW, srcH, destX, destY, dstW, dstH);
        }
    }

    /// <summary>
    /// General affine transform draw using inverse mapping.
    /// </summary>
    private void DrawAffine(Image source, double a, double b, double c, double d, double tx, double ty, int srcW, int srcH, int dstW,
        int dstH)
    {
        double det = (a * d) - (b * c);

        if (Math.Abs(det) < 0.0001)
        {
            return;
        }

        double invDet = 1.0 / det;
        double invA = d * invDet;
        double invB = -b * invDet;
        double invC = -c * invDet;
        double invD = a * invDet;

        for (int dy = 0; dy < dstH; dy++)
        {
            double relY = dy - ty;

            for (int dx = 0; dx < dstW; dx++)
            {
                double relX = dx - tx;
                double srcX = (invA * relX) + (invC * relY);
                double srcY = (invB * relX) + (invD * relY);

                if (srcX >= 0 && srcX < srcW && srcY >= 0 && srcY < srcH)
                {
                    Color pixel = source.GetPixel((int)srcX, (int)srcY);
                    _bitmapData!.SetPixel(dx, dy, pixel);
                }
            }
        }
    }

    private static void BlitStrip(Image source, Image dest, int srcX, int srcY, int w, int h, int destX, int destY, int destW, int destH)
    {
        int sx = Math.Max(0, -destX);
        int sy = Math.Max(0, -destY);
        int ex = Math.Min(w, destW - destX);
        int ey = Math.Min(h, destH - destY);

        if (sx < ex && sy < ey)
        {
            dest.BlitRect(source, new Rect2I(srcX + sx, srcY + sy, ex - sx, ey - sy), new Vector2I(destX + sx, destY + sy));
        }
    }
}
