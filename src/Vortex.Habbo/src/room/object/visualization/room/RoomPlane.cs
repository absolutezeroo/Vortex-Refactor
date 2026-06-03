using System;
using System.Globalization;
using System.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Mask;
using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer;
using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;
using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.RoomPlane
public class RoomPlane : IRoomPlane
{
	public const int TYPE_UNDEFINED = 0;
	public const int TYPE_WALL = 1;
	public const int TYPE_FLOOR = 2;
	public const int TYPE_LANDSCAPE = 3;

	private static int s_nextId = 1;

	private bool _disposed;
	private readonly int _randomSeed;
	private Vector3d? _origin;
	private Vector3d? _location;
	private Vector3d? _leftSide;
	private Vector3d? _rightSide;
	private Vector3d? _normal;
	private readonly List<Vector3d> _secondaryNormals;
	private int _geometryUpdateId = -1;
	private bool _isVisible;
	private IPlaneRasterizer? _rasterizer;
	private PlaneMaskManager? _maskManager;
	private string? _id;
	private readonly double _offsetX;
	private readonly double _offsetY;
	private readonly double _landscapeWidth;
	private readonly double _landscapeHeight;
	private Dictionary<string, PlaneBitmapData>? _textureCache;
	private PlaneBitmapData? _activeTexture;
	private readonly bool _useMask;
	private readonly List<RoomPlaneBitmapMask> _bitmapMasks;
	private readonly List<RoomPlaneRectangleMask> _rectangleMasks;
	private bool _maskChanged;
	private Image? _maskBitmapData;
	private Image? _combinedMaskBitmapData;
	private List<RoomPlaneBitmapMask> _previousBitmapMasks;
	private List<RoomPlaneRectangleMask> _previousRectangleMasks;
	private Vector3d? _cornerA;
	private Vector3d? _cornerB;
	private Vector3d? _cornerC;
	private Vector3d? _cornerD;
	private double _width;
	private double _height;
	private bool _canBeVisible = true;

	private Image? _bitmapData;
	private Vector2? _offset;
	private double _relativeDepth;
	private double _extraDepth;

	public RoomPlane(
		IVector3d origin, IVector3d location, IVector3d leftSide, IVector3d rightSide,
		int type, bool useMask, List<IVector3d>? secondaryNormals, int randomSeed,
		double offsetX = 0, double offsetY = 0, double landscapeWidth = 0, double landscapeHeight = 0)
	{
		_randomSeed = randomSeed;
		_origin = new Vector3d();
		_origin.Assign(origin);
		_location = new Vector3d();
		_location.Assign(location);
		_leftSide = new Vector3d();
		_leftSide.Assign(leftSide);
		_rightSide = new Vector3d();
		_rightSide.Assign(rightSide);
		_normal = Vector3d.CrossProduct(_leftSide, _rightSide);

		if (_normal!.Length > 0)
		{
			_normal.Mul(1.0 / _normal.Length);
		}

		_secondaryNormals = [];

		if (secondaryNormals != null)
		{
			foreach (IVector3d sn in secondaryNormals)
			{
				Vector3d copy = new Vector3d();
				copy.Assign(sn);
				_secondaryNormals.Add(copy);
			}
		}

		_offset = Vector2.Zero;
		Type = type;
		_textureCache = [];
		_cornerA = new Vector3d();
		_cornerB = new Vector3d();
		_cornerC = new Vector3d();
		_cornerD = new Vector3d();
		_offsetX = offsetX;
		_offsetY = offsetY;
		_landscapeWidth = landscapeWidth;
		_landscapeHeight = landscapeHeight;
		_useMask = useMask;
		UniqueId = s_nextId++;
		_bitmapMasks = [];
		_rectangleMasks = [];
		_previousBitmapMasks = [];
		_previousRectangleMasks = [];
	}

	public IVector3d Normal => _normal!;

	public Image? BitmapData
	{
		get
		{
			if (!Visible || _bitmapData == null)
			{
				return null;
			}

			return (Image)_bitmapData.Duplicate();
		}
	}

	public Vector2 Offset => _offset ?? Vector2.Zero;

	public double RelativeDepth => _relativeDepth + _extraDepth;

	public uint Color { get; set; }

	public double ExtraDepth
	{
		set => _extraDepth = value;
	}

	public bool CanBeVisible
	{
		get => _canBeVisible;
		set
		{
			if (value == _canBeVisible)
			{
				return;
			}

			if (!_canBeVisible)
			{
				ResetTextureCache();
			}

			_canBeVisible = value;
		}
	}

	public bool Visible => _isVisible && _canBeVisible;

	public int Type { get; }

	public IVector3d LeftSide => _leftSide!;

	public IVector3d RightSide => _rightSide!;

	public IVector3d Location => _location!;

	public bool HasTexture { get; set; } = true;

	public IPlaneRasterizer? Rasterizer
	{
		set => _rasterizer = value;
	}

	public PlaneMaskManager? MaskManager
	{
		set => _maskManager = value;
	}

	public string? Id
	{
		set
		{
			if (value == _id)
			{
				return;
			}

			ResetTextureCache();

			_id = value;
		}
	}

	public int UniqueId { get; }

	public bool IsHighlighter { get; set; }

	public void Dispose()
	{
		_bitmapData = null;

		if (_textureCache != null)
		{
			foreach (PlaneBitmapData cached in _textureCache.Values)
			{
				cached.Dispose();
			}

			_textureCache = null;
		}

		_activeTexture = null;
		_location = null;
		_origin = null;
		_leftSide = null;
		_rightSide = null;
		_normal = null;
		_rasterizer = null;
		_cornerA = null;
		_cornerB = null;
		_cornerC = null;
		_cornerD = null;
		_maskBitmapData = null;
		_combinedMaskBitmapData = null;

		_disposed = true;
	}

	public Image? CopyBitmapData(Image? destination)
	{
		if (!Visible || _bitmapData == null || destination == null)
		{
			return null;
		}

		if (_bitmapData.GetWidth() != destination.GetWidth() || _bitmapData.GetHeight() != destination.GetHeight())
		{
			return null;
		}

		destination.BlitRect(_bitmapData, new Rect2I(0, 0, _bitmapData.GetWidth(), _bitmapData.GetHeight()), Vector2I.Zero);

		return destination;

	}

	public List<object> GetDrawingDatas(IRoomGeometry geometry)
	{
		List<object> result = [];

		if (!_isVisible)
		{
			return result;
		}

		PlaneDrawingData? maskData = ResolveMasks(geometry);

		object[]? layers = _rasterizer?.GetLayers(_id ?? "");

		if (layers != null)
		{
			foreach (object layer in layers)
			{
				if (layer is not PlaneVisualizationLayer vizLayer)
				{
					continue;
				}

				PlaneDrawingData drawingData;

				if (HasTexture && vizLayer.GetMaterial() != null)
				{
					IVector3d coordPos = geometry.GetCoordinatePosition(Normal);
					PlaneMaterialCellMatrix? matrix = vizLayer.GetMaterial()!.GetMaterialCellMatrix(coordPos);

					if (matrix == null)
					{
						continue;
					}

					drawingData = new PlaneDrawingData(maskData, BlendColor(Color, vizLayer.GetColor()), matrix.IsBottomAligned);

					Randomizer.SetSeed(_randomSeed);

					foreach (PlaneMaterialCellColumn? column in matrix.GetColumns(ScreenWidth(geometry)))
					{
						List<string> assetNames = [];

						assetNames.AddRange(column.GetCells().Select(cell => cell.GetAssetName(coordPos)).OfType<string>());

						if (assetNames.Count <= 0)
						{
							continue;
						}

						if (!column.IsRepeated)
						{
							assetNames.Add("");
						}

						drawingData.AddAssetColumn(assetNames);
					}

					if (drawingData.AssetNameColumns.Count > 0)
					{
						result.Add(drawingData);
					}
				}
				else
				{
					drawingData = new PlaneDrawingData(maskData, BlendColor(Color, vizLayer.GetColor()));

					result.Add(drawingData);
				}
			}
		}

		if (result.Count == 0)
		{
			result.Add(new PlaneDrawingData(maskData, Color));
		}

		return result;
	}

	public bool Update(IRoomGeometry geometry, int timeStamp)
	{
		if (geometry == null || _disposed)
		{
			return false;
		}

		bool geometryChanged = false;

		if (_geometryUpdateId != geometry.UpdateId)
		{
			geometryChanged = true;
		}

		if (!geometryChanged || !_canBeVisible)
		{
			if (!Visible)
			{
				return false;
			}
		}

		switch (geometryChanged)
		{
			case true:
				{
					_activeTexture = null;

					double cosAngle = Vector3d.CosAngle(geometry.DirectionAxis, Normal);

					if (cosAngle > -0.001)
					{
						if (!_isVisible)
						{
							return false;
						}

						_isVisible = false;

						return true;

					}

					foreach (Vector3d sn in _secondaryNormals)
					{
						cosAngle = Vector3d.CosAngle(geometry.DirectionAxis, sn);

						if (!(cosAngle > -0.001))
						{
							continue;
						}

						if (!_isVisible)
						{
							return false;
						}

						_isVisible = false;

						return true;
					}

					UpdateCorners(geometry);

					IVector3d originScreen = geometry.GetScreenPosition(_origin!);
					double depthZ = Math.Max(_cornerA!.Z, Math.Max(_cornerB!.Z, Math.Max(_cornerC!.Z, _cornerD!.Z))) - originScreen.Z;

					switch (Type)
					{
						case TYPE_FLOOR:
							depthZ -= (_location!.Z + Math.Min(0, Math.Min(_leftSide!.Z, _rightSide!.Z))) * 8;

							break;
						case TYPE_LANDSCAPE:
							depthZ += 0.02;

							break;
					}

					_relativeDepth = depthZ;
					_isVisible = true;
					_geometryUpdateId = geometry.UpdateId;

					break;
				}
			case false when !NeedsNewTexture(geometry, timeStamp):
				return false;
		}

		if (_bitmapData == null || _width != _bitmapData.GetWidth() || _height != _bitmapData.GetHeight())
		{
			_bitmapData = null;

			if (_width < 1 || _height < 1)
			{
				return geometryChanged;
			}

			_bitmapData = Image.CreateEmpty((int)_width, (int)_height, false, Image.Format.Rgba8);
			_bitmapData.Fill(new Color(1, 1, 1, 0));
		}
		else
		{
			_bitmapData.Fill(new Color(1, 1, 1, 0));
		}

		Randomizer.SetSeed(_randomSeed);

		Image? texture = GetTexture(geometry, timeStamp);

		if (texture != null)
		{
			RenderTexture(geometry, texture);

			return true;
		}

		Dispose();

		return false;

	}

	public void ResetBitmapMasks()
	{
		if (_disposed || !_useMask)
		{
			return;
		}

		if (_bitmapMasks.Count == 0)
		{
			return;
		}

		_maskChanged = true;
		_bitmapMasks.Clear();
	}

	public bool AddBitmapMask(string type, double leftSideLoc, double rightSideLoc)
	{
		if (!_useMask)
		{
			return false;
		}

		if (_bitmapMasks.Any(existing =>
				existing.Type == type && existing.LeftSideLoc == leftSideLoc && existing.RightSideLoc == rightSideLoc))
		{
			return false;
		}

		_bitmapMasks.Add(new RoomPlaneBitmapMask(type, leftSideLoc, rightSideLoc));
		_maskChanged = true;

		return true;
	}

	public void ResetRectangleMasks()
	{
		if (!_useMask)
		{
			return;
		}

		if (_rectangleMasks.Count == 0)
		{
			return;
		}

		_maskChanged = true;
		_rectangleMasks.Clear();
	}

	public bool AddRectangleMask(double leftSideLoc, double rightSideLoc,
		double leftSideLength, double rightSideLength)
	{
		if (!_useMask)
		{
			return false;
		}

		if (_rectangleMasks.Any(existing => existing.LeftSideLoc == leftSideLoc && existing.RightSideLoc == rightSideLoc &&
											existing.LeftSideLength == leftSideLength && existing.RightSideLength == rightSideLength))
		{
			return false;
		}

		_rectangleMasks.Add(new RoomPlaneRectangleMask(leftSideLoc, rightSideLoc, leftSideLength, rightSideLength));
		_maskChanged = true;

		return true;
	}

	private bool CacheTexture(string key, PlaneBitmapData? data)
	{
		if (_textureCache != null && _textureCache.Remove(key, out PlaneBitmapData? old))
		{
			old.Dispose();
		}

		_activeTexture = data;

		if (_textureCache != null)
		{
			_textureCache[key] = data!;
		}

		return true;
	}

	private void ResetTextureCache(Image? preserve = null)
	{
		if (_textureCache != null)
		{
			foreach (PlaneBitmapData cached in _textureCache.Values)
			{
				cached.Dispose();
			}

			_textureCache.Clear();
		}

		_activeTexture = null;
	}

	private string GetTextureIdentifier(double scale)
	{
		if (_rasterizer != null)
		{
			return _rasterizer.GetTextureIdentifier(scale, Normal);
		}

		return scale.ToString(CultureInfo.CurrentCulture);
	}

	private bool NeedsNewTexture(IRoomGeometry geometry, int timeStamp)
	{
		PlaneBitmapData? cached = _activeTexture;

		if (cached == null)
		{
			string key = GetTextureIdentifier(geometry.Scale);
			_textureCache?.TryGetValue(key, out cached);
		}

		UpdateMaskChangeStatus();

		return _canBeVisible && (cached == null || (cached.TimeStamp >= 0 && timeStamp > cached.TimeStamp) || _maskChanged);
	}

	private Image? GetTexture(IRoomGeometry geometry, int timeStamp)
	{
		PlaneBitmapData? cached = null;
		string? key = null;

		if (NeedsNewTexture(geometry, timeStamp))
		{
			double leftLength = _leftSide!.Length * geometry.Scale;
			double rightLength = _rightSide!.Length * geometry.Scale;
			IVector3d coordPos = geometry.GetCoordinatePosition(Normal);
			key = GetTextureIdentifier(geometry.Scale);

			if (_activeTexture != null)
			{
				cached = _activeTexture;
			}
			else
			{
				_textureCache?.TryGetValue(key, out cached);
			}

			Image? existingBitmap = cached?.Bitmap;

			if (_rasterizer != null)
			{
				PlaneBitmapData? rendered = _rasterizer.Render(
					existingBitmap, _id ?? "", leftLength, rightLength, geometry.Scale, coordPos,
					HasTexture, _offsetX, _offsetY, _landscapeWidth, _landscapeHeight, timeStamp);

				if (rendered != null)
				{
					cached = rendered;
				}
			}
			else
			{
				Image? solidBitmap = Image.CreateEmpty((int)leftLength, (int)rightLength, false, Image.Format.Rgba8);
				float r = ((Color >> 16) & 0xFF) / 255f;
				float g = ((Color >> 8) & 0xFF) / 255f;
				float b = (Color & 0xFF) / 255f;
				solidBitmap.Fill(new Color(r, g, b, 1f));
				cached = new PlaneBitmapData(solidBitmap, -1);
			}

			if (cached != null)
			{
				UpdateMask(cached.Bitmap, geometry);
				CacheTexture(key!, cached);
			}
		}
		else if (_activeTexture != null)
		{
			cached = _activeTexture;
		}
		else
		{
			key = GetTextureIdentifier(geometry.Scale);
			_textureCache?.TryGetValue(key, out cached);
		}

		if (cached == null)
		{
			return null;
		}

		_activeTexture = cached;

		return cached.Bitmap;

	}

	private PlaneDrawingData? ResolveMasks(IRoomGeometry geometry)
	{
		if (!_useMask || _maskManager == null)
		{
			return null;
		}

		PlaneDrawingData drawingData = new PlaneDrawingData();

		foreach (RoomPlaneBitmapMask bitmapMask in _bitmapMasks)
		{
			PlaneMask? mask = _maskManager.GetMask(bitmapMask.Type);

			if (mask == null)
			{
				continue;
			}

			string? assetName = mask.GetAssetName((int)geometry.Scale);

			if (assetName == null)
			{
				continue;
			}

			IVector3d coordPos = geometry.GetCoordinatePosition(Normal);
			IGraphicAsset? asset = mask.GetGraphicAsset(geometry.Scale, coordPos);

			if (asset == null || _maskBitmapData == null)
			{
				continue;
			}

			int posX = (int)(_maskBitmapData.GetWidth() * (1.0 - bitmapMask.LeftSideLoc / _leftSide!.Length));
			int posY = (int)(_maskBitmapData.GetHeight() * (1.0 - bitmapMask.RightSideLoc / _rightSide!.Length));

			drawingData.AddMask(assetName, new Vector2I(posX + asset.OffsetX, posY + asset.OffsetY),
				asset.FlipH, asset.FlipV);
		}

		return drawingData;
	}

	private int ScreenWidth(IRoomGeometry geometry)
	{
		Vector2? a = geometry.GetScreenPoint(new Vector3d(0, 0, 0));
		Vector2? b = geometry.GetScreenPoint(new Vector3d(0, 1, 0));

		if (a == null || b == null)
		{
			return 0;
		}

		return (int)Math.Round(_leftSide!.Length * Math.Abs(a.Value.X - b.Value.X));
	}

	private void UpdateCorners(IRoomGeometry geometry)
	{
		_cornerA!.Assign(geometry.GetScreenPosition(_location!));
		_cornerB!.Assign(geometry.GetScreenPosition(Vector3d.Sum(_location!, _rightSide!)));
		_cornerC!.Assign(geometry.GetScreenPosition(Vector3d.Sum(Vector3d.Sum(_location!, _leftSide!), _rightSide!)));
		_cornerD!.Assign(geometry.GetScreenPosition(Vector3d.Sum(_location!, _leftSide!)));

		_offset = geometry.GetScreenPoint(_origin!);

		_cornerA.X = Math.Round(_cornerA.X);
		_cornerA.Y = Math.Round(_cornerA.Y);
		_cornerB.X = Math.Round(_cornerB.X);
		_cornerB.Y = Math.Round(_cornerB.Y);
		_cornerC.X = Math.Round(_cornerC.X);
		_cornerC.Y = Math.Round(_cornerC.Y);
		_cornerD.X = Math.Round(_cornerD.X);
		_cornerD.Y = Math.Round(_cornerD.Y);

		if (_offset.HasValue)
		{
			_offset = new Vector2((float)Math.Round(_offset.Value.X), (float)Math.Round(_offset.Value.Y));
		}

		double minX = Math.Min(_cornerA.X, Math.Min(_cornerB.X, Math.Min(_cornerC.X, _cornerD.X)));
		double maxX = Math.Max(_cornerA.X, Math.Max(_cornerB.X, Math.Max(_cornerC.X, _cornerD.X)));
		double minY = Math.Min(_cornerA.Y, Math.Min(_cornerB.Y, Math.Min(_cornerC.Y, _cornerD.Y)));
		double maxY = Math.Max(_cornerA.Y, Math.Max(_cornerB.Y, Math.Max(_cornerC.Y, _cornerD.Y)));

		double w = maxX - minX;

		if (_offset.HasValue)
		{
			_offset = new Vector2((float)(_offset.Value.X - minX), (float)(_offset.Value.Y - minY));
		}

		_cornerA.X -= minX;
		_cornerB.X -= minX;
		_cornerC.X -= minX;
		_cornerD.X -= minX;
		_cornerA.Y -= minY;
		_cornerB.Y -= minY;
		_cornerC.Y -= minY;
		_cornerD.Y -= minY;

		_width = w;
		_height = maxY - minY;
	}

	private void RenderTexture(IRoomGeometry geometry, Image texture)
	{
		if (_cornerA == null || _cornerB == null || _cornerC == null || _cornerD == null ||
			texture == null || _bitmapData == null)
		{
			return;
		}

		double dx = _cornerD.X - _cornerC.X;
		double dy = _cornerD.Y - _cornerC.Y;
		double bx = _cornerB.X - _cornerC.X;
		double by = _cornerB.Y - _cornerC.Y;

		int tw = texture.GetWidth();
		int th = texture.GetHeight();

		if (tw == 0 || th == 0)
		{
			return;
		}

		// Snap near-integer values for walls/landscapes
		if (Type is TYPE_WALL or TYPE_LANDSCAPE)
		{
			if (Math.Abs(bx - tw) <= 1) { bx = tw; }
			if (Math.Abs(by - tw) <= 1) { by = tw; }
			if (Math.Abs(dx - th) <= 1) { dx = th; }
			if (Math.Abs(dy - th) <= 1) { dy = th; }
		}

		double a = bx / tw;
		double b = by / tw;
		double c = dx / th;
		double d = dy / th;

		Draw(texture, a, b, c, d, _cornerC.X, _cornerC.Y);
	}

	/// @see com.sulake.habbo.room.object.visualization.room.RoomPlane#draw
	private void Draw(Image source, double a, double b, double c, double d, double tx, double ty)
	{
		if (_bitmapData == null)
		{
			return;
		}

		int sw = source.GetWidth();
		int sh = source.GetHeight();
		int dw = _bitmapData.GetWidth();
		int dh = _bitmapData.GetHeight();

		// Optimized skew path for walls/landscapes with unit scale
		if (Math.Abs(a - 1) < 0.001 && Math.Abs(d - 1) < 0.001 && Math.Abs(c) < 0.001 &&
			Math.Abs(b) > 0.001 && Math.Abs(b) <= 1 &&
			Type is TYPE_WALL or TYPE_LANDSCAPE)
		{
			int col = 0;
			double accum = 0;
			int prevCol = 0;
			int yShift = 0;

			if (b > 0)
			{
				ty++;
			}

			while (col < sw)
			{
				col++;
				accum += Math.Abs(b);

				if (!(accum >= 1))
				{
					continue;
				}

				// Copy a strip from source to destination with skew
				int stripWidth = col - prevCol;
				int stripX = prevCol;
				int destX = (int)tx + prevCol;
				int destY = (int)ty + yShift;

				for (int y = 0; y < sh; y++)
				{
					int dy2 = destY + y;

					if (dy2 < 0 || dy2 >= dh)
					{
						continue;
					}

					for (int x = 0; x < stripWidth; x++)
					{
						int sx = stripX + x;
						int dx2 = destX + x;

						if (sx < 0 || sx >= sw || dx2 < 0 || dx2 >= dw)
						{
							continue;
						}

						Color pixel = source.GetPixel(sx, y);

						if (pixel.A > 0)
						{
							_bitmapData.SetPixel(dx2, dy2, pixel);
						}
					}
				}

				prevCol = col;
				yShift += b > 0 ? 1 : -1;
				accum = 0;
			}

			if (accum > 0)
			{
				int stripWidth = col - prevCol;
				int stripX = prevCol;
				int destX = (int)tx + prevCol;
				int destY = (int)ty + yShift;

				for (int y = 0; y < sh; y++)
				{
					int dy2 = destY + y;

					if (dy2 < 0 || dy2 >= dh)
					{
						continue;
					}

					for (int x = 0; x < stripWidth; x++)
					{
						int sx = stripX + x;
						int dx2 = destX + x;

						if (sx < 0 || sx >= sw || dx2 < 0 || dx2 >= dw)
						{
							continue;
						}

						Color pixel = source.GetPixel(sx, y);

						if (pixel.A > 0)
						{
							_bitmapData.SetPixel(dx2, dy2, pixel);
						}
					}
				}
			}

			return;
		}

		// General affine transform
		double det = a * d - b * c;

		if (Math.Abs(det) < 0.0001)
		{
			return;
		}

		double invDet = 1.0 / det;
		double invA = d * invDet;
		double invB = -b * invDet;
		double invC = -c * invDet;
		double invD = a * invDet;

		for (int dy3 = 0; dy3 < dh; dy3++)
		{
			for (int dx3 = 0; dx3 < dw; dx3++)
			{
				double localX = dx3 - tx;
				double localY = dy3 - ty;

				double srcX = localX * invA + localY * invC;
				double srcY = localX * invB + localY * invD;

				int sx = (int)srcX;
				int sy = (int)srcY;

				if (sx < 0 || sx >= sw || sy < 0 || sy >= sh)
				{
					continue;
				}

				Color pixel = source.GetPixel(sx, sy);

				if (pixel.A > 0)
				{
					_bitmapData.SetPixel(dx3, dy3, pixel);
				}
			}
		}
	}

	private void UpdateMaskChangeStatus()
	{
		if (!_maskChanged)
		{
			return;
		}

		bool masksMatch = true;

		if (_bitmapMasks.Count == _previousBitmapMasks.Count)
		{
			if (_bitmapMasks.Select(current => _previousBitmapMasks.Any(prev =>
								prev.Type == current.Type && prev.LeftSideLoc == current.LeftSideLoc &&
								prev.RightSideLoc == current.RightSideLoc))
							.Any(found => !found))
			{
				masksMatch = false;
			}
		}
		else
		{
			masksMatch = false;
		}

		if (_rectangleMasks.Count > _previousRectangleMasks.Count)
		{
			masksMatch = false;
		}

		if (masksMatch)
		{
			_maskChanged = false;
		}
	}

	private void UpdateMask(Image? textureBitmap, IRoomGeometry geometry)
	{
		bool hasBitmapMasks = _bitmapMasks.Count > 0;
		bool hasRectangleMasks = _rectangleMasks.Count > 0;

		if (!_useMask || (!hasBitmapMasks && !hasRectangleMasks && !_maskChanged))
		{
			return;
		}

		// Bitmap masks require a MaskManager; rectangle masks are self-contained.
		// If only bitmap masks exist and there's no manager, bail out.
		if (!hasRectangleMasks && _maskManager == null)
		{
			return;
		}

		if (textureBitmap == null)
		{
			return;
		}

		UpdateMaskChangeStatus();

		int tw = textureBitmap.GetWidth();
		int th = textureBitmap.GetHeight();

		if (_maskBitmapData == null || _maskBitmapData.GetWidth() != tw || _maskBitmapData.GetHeight() != th)
		{
			_maskBitmapData = Image.CreateEmpty(tw, th, false, Image.Format.Rgba8);
			_maskBitmapData.Fill(Colors.White);
			_maskChanged = true;
		}

		if (_maskChanged)
		{
			_previousBitmapMasks = [];
			_previousRectangleMasks = [];

			_maskBitmapData.Fill(Colors.White);
			ResetTextureCache(textureBitmap);

			IVector3d coordPos = geometry.GetCoordinatePosition(Normal);

			if (_maskManager != null)
			{
				foreach (RoomPlaneBitmapMask bitmapMask in _bitmapMasks)
				{
					int posX = (int)(tw - tw * bitmapMask.LeftSideLoc / _leftSide!.Length);
					int posY = (int)(th - th * bitmapMask.RightSideLoc / _rightSide!.Length);

					_maskManager.UpdateMask(_maskBitmapData, bitmapMask.Type, geometry.Scale, coordPos, posX, posY);
					_previousBitmapMasks.Add(new RoomPlaneBitmapMask(bitmapMask.Type, bitmapMask.LeftSideLoc, bitmapMask.RightSideLoc));
				}
			}

			foreach (RoomPlaneRectangleMask rectMask in _rectangleMasks)
			{
				int posX = (int)(tw - tw * rectMask.LeftSideLoc / _leftSide!.Length);
				int posY = (int)(th - th * rectMask.RightSideLoc / _rightSide!.Length);
				int rectW = (int)(tw * rectMask.LeftSideLength / _leftSide.Length);
				int rectH = (int)(th * rectMask.RightSideLength / _rightSide!.Length);

				for (int y = posY - rectH; y < posY; y++)
				{
					for (int x = posX - rectW; x < posX; x++)
					{
						if (x >= 0 && x < tw && y >= 0 && y < th)
						{
							_maskBitmapData.SetPixel(x, y, Colors.Black);
						}
					}
				}

				_previousRectangleMasks.Add(new RoomPlaneRectangleMask(
					rectMask.LeftSideLoc, rectMask.RightSideLoc, rectMask.LeftSideLength, rectMask.RightSideLength));
			}

			_maskChanged = false;
		}

		CombineTextureMask(textureBitmap, _maskBitmapData);
	}

	/// @see com.sulake.habbo.room.object.visualization.room.RoomPlane#combineTextureMask
	private void CombineTextureMask(Image texture, Image mask)
	{
		int tw = texture.GetWidth();
		int th = texture.GetHeight();

		if (_combinedMaskBitmapData != null &&
			(_combinedMaskBitmapData.GetWidth() != tw || _combinedMaskBitmapData.GetHeight() != th))
		{
			_combinedMaskBitmapData = null;
		}

		_combinedMaskBitmapData ??= Image.CreateEmpty(tw, th, false, Image.Format.Rgba8);
		_combinedMaskBitmapData.Fill(Colors.White);

		// Copy alpha channel from texture to red channel of combined mask
		// Then darken with mask
		// Then copy red channel back to alpha
		for (int y = 0; y < th; y++)
		{
			for (int x = 0; x < tw; x++)
			{
				Color texPixel = texture.GetPixel(x, y);
				Color maskPixel = mask.GetPixel(x, y);

				// Darken: take min of texture alpha and mask red
				float maskedAlpha = Math.Min(texPixel.A, maskPixel.R);
				texture.SetPixel(x, y, new Color(texPixel.R, texPixel.G, texPixel.B, maskedAlpha));
			}
		}
	}

	private static uint BlendColor(uint baseColor, uint layerColor)
	{
		uint bR = (baseColor >> 16) & 0xFF;
		uint bG = (baseColor >> 8) & 0xFF;
		uint bB = baseColor & 0xFF;

		uint lR = (layerColor >> 16) & 0xFF;
		uint lG = (layerColor >> 8) & 0xFF;
		uint lB = layerColor & 0xFF;

		uint rR = lR * bR / 255;
		uint rG = lG * bG / 255;
		uint rB = lB * bB / 255;

		return (rR << 16) | (rG << 8) | rB;
	}
}
