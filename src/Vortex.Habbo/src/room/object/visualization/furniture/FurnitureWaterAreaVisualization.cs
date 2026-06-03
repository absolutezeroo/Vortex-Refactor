using System;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureWaterAreaVisualization
public class FurnitureWaterAreaVisualization : AnimatedFurnitureVisualization
{
    private const string SHORE_SPRITE_TAG = "shore";

    private bool _hasShore = true;
    private List<bool> _borderActive = [];
    private List<int> _borderTypes = [];
    private List<int> _registeredSizes = [];
    private bool _needsShoreUpdate;
    private int _sizeX;
    private int _sizeY;
    private int _cachedShoreSpriteIndex;
    private int _cachedShoreScale = -1;
    private int _cachedShoreDirection = -1;
    private Image? _maskBuffer;

    public override void Dispose()
    {
        if (AssetCollection != null && Object != null)
        {
            foreach (int size in _registeredSizes)
            {
                ShoreMaskCreatorUtility.DisposeInstanceMask(Object.Id, size, AssetCollection);
            }

            _registeredSizes = [];
        }

        _maskBuffer = null;
        base.Dispose();
    }

    protected override bool UpdateObject(double scale, double geometryDirection)
    {
        if (base.UpdateObject(scale, geometryDirection))
        {
            _needsShoreUpdate = true;
            UpdateBorderData();
            return true;
        }

        return false;
    }

    protected override int UpdateAnimation(double scale)
    {
        int result = base.UpdateAnimation(scale);

        if (UpdateInstanceShoreMask(scale))
        {
            int shoreSpriteIndex = GetShoreSpriteIndex((int)scale);
            result |= 1 << shoreSpriteIndex;
        }

        return result;
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        if (scale == 1 || spriteIndex != GetShoreSpriteIndex(scale))
        {
            return base.GetSpriteAssetName(scale, spriteIndex);
        }

        if (_hasShore && Object != null)
        {
            return ShoreMaskCreatorUtility.GetInstanceMaskName(Object.Id, GetSize(scale));
        }

        return null;
    }

    protected override void SetAnimation(int animationId)
    {
        base.SetAnimation(0);
    }

    private int GetShoreSpriteIndex(int scale)
    {
        if (_cachedShoreScale == scale && _cachedShoreDirection == Direction)
        {
            return _cachedShoreSpriteIndex;
        }

        for (int i = SpriteCount - 1; i >= 0; i--)
        {
            if (GetSpriteTag(scale, Direction, i) == SHORE_SPRITE_TAG)
            {
                _cachedShoreSpriteIndex = i;
                _cachedShoreScale = scale;
                _cachedShoreDirection = Direction;
                return _cachedShoreSpriteIndex;
            }
        }

        return -1;
    }

    private IGraphicAsset? GetShoreAsset(int scale)
    {
        string? assetName = base.GetSpriteAssetName(scale, GetShoreSpriteIndex(scale));

        if (assetName == null)
        {
            return null;
        }

        return AssetCollection?.GetAsset(assetName);
    }

    private IGraphicAsset? GetInstanceMask(int scale)
    {
        IRoomObject? obj = Object;

        if (obj == null || AssetCollection == null)
        {
            return null;
        }

        int size = GetSize((int)scale);
        IGraphicAsset? mask = ShoreMaskCreatorUtility.GetInstanceMask(obj.Id, size, AssetCollection, GetShoreAsset((int)scale));

        if (mask != null && !_registeredSizes.Contains(size))
        {
            _registeredSizes.Add(size);
        }

        return mask;
    }

    private void UpdateBorderData()
    {
        ResetBorders();

        IRoomObject? obj = Object;

        if (obj == null)
        {
            return;
        }

        int state = obj.GetState(0);
        bool[][] areaData = GetAreaData();
        int cols = _sizeX + 2;
        int rows = _sizeY + 2;

        bool[] bottomRow = areaData[rows - 1];

        for (int c = cols - 1; c >= 0; c--)
        {
            if ((state & 1) != 0)
            {
                bottomRow[c] = true;
            }

            state >>= 1;
        }

        for (int r = rows - 2; r >= 1; r--)
        {
            bool[] row = areaData[r];

            if ((state & 1) != 0)
            {
                row[cols - 1] = true;
            }

            state >>= 1;

            if ((state & 1) != 0)
            {
                row[0] = true;
            }

            state >>= 1;
        }

        bool[] topRow = areaData[0];

        for (int c = cols - 1; c >= 0; c--)
        {
            if ((state & 1) != 0)
            {
                topRow[c] = true;
            }

            state >>= 1;
        }

        int borderIndex = 0;
        borderIndex = UpdateTopBorder(areaData, borderIndex);
        borderIndex = UpdateRightBorder(areaData, borderIndex);
        borderIndex = UpdateBottomBorder(areaData, borderIndex);
        UpdateLeftBorder(areaData, borderIndex);

        _hasShore = false;

        for (int i = 0; i < _borderActive.Count; i++)
        {
            if (_borderActive[i])
            {
                _hasShore = true;
                break;
            }
        }
    }

    private int UpdateTopBorder(bool[][] areaData, int borderIndex)
    {
        int cols = _sizeX + 2;
        bool[] topRow = areaData[0];
        bool[] secondRow = areaData[1];

        for (int c = 1; c < cols - 1; c++)
        {
            if (!topRow[c])
            {
                _borderActive[borderIndex] = true;

                int leftCut;

                if (!secondRow[c - 1] && !topRow[c - 1])
                {
                    leftCut = 0;
                }
                else if (topRow[c - 1])
                {
                    leftCut = 2;
                }
                else
                {
                    leftCut = 1;
                }

                int rightCut;

                if (!secondRow[c + 1] && !topRow[c + 1])
                {
                    rightCut = 0;
                }
                else if (topRow[c + 1])
                {
                    rightCut = 2;
                }
                else
                {
                    rightCut = 1;
                }

                _borderTypes[borderIndex] = ShoreMaskCreatorUtility.GetBorderType(leftCut, rightCut);
            }

            borderIndex++;
        }

        return borderIndex;
    }

    private int UpdateRightBorder(bool[][] areaData, int borderIndex)
    {
        int cols = _sizeX + 2;
        int rows = _sizeY + 2;

        for (int r = 1; r < rows - 1; r++)
        {
            bool[] row = areaData[r];
            bool[] prevRow = areaData[r - 1];
            bool[] nextRow = areaData[r + 1];

            if (!row[cols - 1])
            {
                _borderActive[borderIndex] = true;

                int leftCut;

                if (!prevRow[cols - 2] && !prevRow[cols - 1])
                {
                    leftCut = 0;
                }
                else if (prevRow[cols - 1])
                {
                    leftCut = 2;
                }
                else
                {
                    leftCut = 1;
                }

                int rightCut;

                if (!nextRow[cols - 2] && !nextRow[cols - 1])
                {
                    rightCut = 0;
                }
                else if (nextRow[cols - 1])
                {
                    rightCut = 2;
                }
                else
                {
                    rightCut = 1;
                }

                _borderTypes[borderIndex] = ShoreMaskCreatorUtility.GetBorderType(leftCut, rightCut);
            }

            borderIndex++;
        }

        return borderIndex;
    }

    private int UpdateBottomBorder(bool[][] areaData, int borderIndex)
    {
        int cols = _sizeX + 2;
        int rows = _sizeY + 2;
        bool[] bottomRow = areaData[rows - 1];
        bool[] prevRow = areaData[rows - 2];

        for (int c = cols - 2; c >= 1; c--)
        {
            if (!bottomRow[c])
            {
                _borderActive[borderIndex] = true;

                int leftCut;

                if (!prevRow[c + 1] && !bottomRow[c + 1])
                {
                    leftCut = 0;
                }
                else if (bottomRow[c + 1])
                {
                    leftCut = 2;
                }
                else
                {
                    leftCut = 1;
                }

                int rightCut;

                if (!prevRow[c - 1] && !bottomRow[c - 1])
                {
                    rightCut = 0;
                }
                else if (bottomRow[c - 1])
                {
                    rightCut = 2;
                }
                else
                {
                    rightCut = 1;
                }

                _borderTypes[borderIndex] = ShoreMaskCreatorUtility.GetBorderType(leftCut, rightCut);
            }

            borderIndex++;
        }

        return borderIndex;
    }

    private int UpdateLeftBorder(bool[][] areaData, int borderIndex)
    {
        int rows = _sizeY + 2;

        for (int r = rows - 2; r >= 1; r--)
        {
            bool[] row = areaData[r];
            bool[] nextRow = areaData[r + 1];
            bool[] prevRow = areaData[r - 1];

            if (!row[0])
            {
                _borderActive[borderIndex] = true;

                int leftCut;

                if (!nextRow[1] && !nextRow[0])
                {
                    leftCut = 0;
                }
                else if (nextRow[0])
                {
                    leftCut = 2;
                }
                else
                {
                    leftCut = 1;
                }

                int rightCut;

                if (!prevRow[1] && !prevRow[0])
                {
                    rightCut = 0;
                }
                else if (prevRow[0])
                {
                    rightCut = 2;
                }
                else
                {
                    rightCut = 1;
                }

                _borderTypes[borderIndex] = ShoreMaskCreatorUtility.GetBorderType(leftCut, rightCut);
            }

            borderIndex++;
        }

        return borderIndex;
    }

    private void ResetBorders()
    {
        if (_sizeX == 0 || _sizeY == 0)
        {
            IRoomObject? obj = Object;

            if (obj == null)
            {
                return;
            }

            IRoomObjectModel model = obj.Model;
            _sizeX = (int)model.GetNumber("furniture_size_x");
            _sizeY = (int)model.GetNumber("furniture_size_y");
        }

        int totalBorders = (_sizeX * 2) + (_sizeY * 2);
        _borderActive = new List<bool>(totalBorders);
        _borderTypes = new List<int>(totalBorders);

        for (int i = 0; i < totalBorders; i++)
        {
            _borderActive.Add(false);
            _borderTypes.Add(1);
        }
    }

    private bool[][] GetAreaData()
    {
        int cols = _sizeX + 2;
        int rows = _sizeY + 2;
        bool[][] data = new bool[rows][];

        for (int r = 0; r < rows; r++)
        {
            data[r] = new bool[cols];
        }

        for (int r = 1; r < rows - 1; r++)
        {
            for (int c = 1; c < cols - 1; c++)
            {
                data[r][c] = true;
            }
        }

        return data;
    }

    private bool InitializeShoreMasks(double scale)
    {
        return ShoreMaskCreatorUtility.InitializeShoreMasks(GetSize((int)scale), AssetCollection, GetShoreAsset((int)scale));
    }

    private Image? CreateShoreMask(int width, int height, double scale)
    {
        if (_maskBuffer == null || _maskBuffer.GetWidth() < width || _maskBuffer.GetHeight() < height)
        {
            _maskBuffer = ShoreMaskCreatorUtility.CreateEmptyMask(width, height);
        }

        if (AssetCollection == null)
        {
            return null;
        }

        return ShoreMaskCreatorUtility.CreateShoreMask2x2(_maskBuffer, GetSize((int)scale), _borderActive, _borderTypes, AssetCollection);
    }

    private bool UpdateInstanceShoreMask(double scale)
    {
        if (!_needsShoreUpdate)
        {
            return false;
        }

        IGraphicAsset? instanceMask = GetInstanceMask((int)scale);

        if (instanceMask?.Asset?.Content is not Image maskImage)
        {
            return false;
        }

        if (!InitializeShoreMasks(scale))
        {
            return false;
        }

        int maskW = maskImage.GetWidth();
        int maskH = maskImage.GetHeight();
        Image? shoreMask = CreateShoreMask(maskW, maskH, scale);
        IGraphicAsset? shoreAsset = GetShoreAsset((int)scale);

        if (shoreAsset?.Asset?.Content is Image shoreImage && shoreMask != null)
        {
            maskImage.Fill(new Color(0, 0, 0, 0));
            ApplyMaskedCopy(maskImage, shoreImage, shoreMask);
            _needsShoreUpdate = false;
        }

        return true;
    }

    private static void ApplyMaskedCopy(Image dest, Image source, Image mask)
    {
        int w = Math.Min(dest.GetWidth(), Math.Min(source.GetWidth(), mask.GetWidth()));
        int h = Math.Min(dest.GetHeight(), Math.Min(source.GetHeight(), mask.GetHeight()));

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color maskPixel = mask.GetPixel(x, y);

                if (maskPixel.A > 0)
                {
                    dest.SetPixel(x, y, source.GetPixel(x, y));
                }
            }
        }
    }
}
