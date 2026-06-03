using System;

using Godot;

using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.ShoreMaskCreatorUtility
public static class ShoreMaskCreatorUtility
{
    public const int NO_CUT = 0;
    public const int STRAIGHT_CUT = 1;
    public const int INNER_CUT = 2;

    private const int CUT_TYPE_COUNT = 3;
    private static readonly Color TRANSPARENT = new(0, 0, 0, 0);
    private static readonly Color SOLID = Colors.White;

    public static Image CreateEmptyMask(int width, int height)
    {
        return Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
    }

    public static string GetInstanceMaskName(int instanceId, int size)
    {
        return "instance_mask_" + instanceId + "_" + size;
    }

    public static int GetBorderType(int a, int b)
    {
        return a + (b * CUT_TYPE_COUNT);
    }

    public static IGraphicAsset? GetInstanceMask(int instanceId, int size, IGraphicAssetCollection collection, IGraphicAsset? shoreAsset)
    {
        string name = GetInstanceMaskName(instanceId, size);
        IGraphicAsset? asset = collection.GetAsset(name);

        if (asset == null && shoreAsset?.Asset?.Content is Image sourceImage)
        {
            int w = sourceImage.GetWidth();
            int h = sourceImage.GetHeight();
            Image mask = CreateEmptyMask(w, h);
            collection.AddAsset(name, mask, false, shoreAsset.OffsetX, shoreAsset.OffsetY);
            asset = collection.GetAsset(name);
        }

        return asset;
    }

    public static void DisposeInstanceMask(int instanceId, int size, IGraphicAssetCollection collection)
    {
        string name = GetInstanceMaskName(instanceId, size);
        collection.DisposeAsset(name);
    }

    public static Image? CreateShoreMask2x2(Image buffer, int size, List<bool> borderActive, List<int> borderTypes,
        IGraphicAssetCollection collection)
    {
        buffer.Fill(TRANSPARENT);

        for (int i = 0; i < borderActive.Count; i++)
        {
            if (!borderActive[i])
            {
                continue;
            }

            string maskName = "mask_" + size + "_" + i + "_" + borderTypes[i];
            IGraphicAsset? maskAsset = collection.GetAsset(maskName);

            if (maskAsset?.Asset?.Content is Image maskImage)
            {
                CopyPixelsWithMask(buffer, maskImage);
            }
        }

        return buffer;
    }

    public static bool InitializeShoreMasks(int size, IGraphicAssetCollection? collection, IGraphicAsset? shoreAsset)
    {
        if (collection == null)
        {
            return false;
        }

        string doneKey = "masks_done_" + size;

        if (collection.GetAsset(doneKey) != null)
        {
            return true;
        }

        if (shoreAsset?.Asset?.Content is not Image sourceImage)
        {
            return false;
        }

        int w = sourceImage.GetWidth();
        int h = sourceImage.GetHeight();

        int[] leftCuts = [0, 1, 2, 0, 1, 2];
        int[] rightCuts = [1, 1, 1, 2, 2, 2];

        for (int i = 0; i < leftCuts.Length; i++)
        {
            Image leftMask = CreateMaskLeft(w, h);
            CutLeftMask(leftMask, size, leftCuts[i], rightCuts[i]);
            StoreLeftMask(collection, leftMask, size, leftCuts[i], rightCuts[i]);

            Image rightMask = CreateMaskRight(w, h);
            CutRightMask(rightMask, size, rightCuts[i], leftCuts[i]);
            StoreRightMask(collection, rightMask, size, rightCuts[i], leftCuts[i]);
        }

        Image marker = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        collection.AddAsset(doneKey, marker, false);
        return true;
    }

    private static Image CreateMaskLeft(int width, int height)
    {
        Image mask = CreateEmptyMask(width, height);
        FillTopLeftCorner(mask, width / 2, (height / 2) - 1, 1, SOLID);
        return mask;
    }

    private static void CutLeftMask(Image mask, int size, int leftCut, int rightCut)
    {
        switch (leftCut)
        {
            case STRAIGHT_CUT:
                CutLeftMaskOuterCorner(mask, size, false);

                break;
            case INNER_CUT:
                CutLeftMaskOuterCorner(mask, size, true);

                break;
        }

        if (rightCut == INNER_CUT)
        {
            CutLeftMaskInnerCorner(mask, size);
        }
    }

    private static void CutLeftMaskOuterCorner(Image mask, int size, bool inner)
    {
        int h = mask.GetHeight();
        int w = mask.GetWidth();
        int cutY = (h / 2) - (size / 2);
        int midX = w / 2;

        if (inner)
        {
            mask.FillRect(new Rect2I(midX, 0, w - midX, cutY), TRANSPARENT);
        }
        else
        {
            FillTopLeftCorner(mask, midX, cutY - 1, 1, TRANSPARENT);
        }
    }

    private static void CutLeftMaskInnerCorner(Image mask, int size)
    {
        int w = mask.GetWidth();
        int h = mask.GetHeight();
        int cutX = (w / 2) + (size / 2);

        mask.FillRect(new Rect2I(cutX, 0, w - cutX, h / 2), TRANSPARENT);
    }

    private static Image CreateMaskRight(int width, int height)
    {
        Image mask = CreateEmptyMask(width, height);

        FillBottomRightCorner(mask, (width / 2) + 1, (height / 2) - 1, SOLID);

        return mask;
    }

    private static void CutRightMask(Image mask, int size, int leftCut, int rightCut)
    {
        switch (rightCut)
        {
            case STRAIGHT_CUT:
                CutRightMaskOuterCorner(mask, size, false);

                break;
            case INNER_CUT:
                CutRightMaskOuterCorner(mask, size, true);

                break;
        }

        if (leftCut == INNER_CUT)
        {
            CutRightMaskInnerCorner(mask, size);
        }
    }

    private static void CutRightMaskInnerCorner(Image mask, int size)
    {
        int w = mask.GetWidth();
        int h = mask.GetHeight();
        int cutX = (w / 2) + (size / 2);

        mask.FillRect(new Rect2I(cutX, 0, w - cutX, (h / 2) - (size / 4)), TRANSPARENT);
    }

    private static void CutRightMaskOuterCorner(Image mask, int size, bool inner)
    {
        int h = mask.GetHeight();
        int w = mask.GetWidth();
        int cutY = h / 2;
        int cutX = (w / 2) + size;

        if (inner)
        {
            mask.FillRect(new Rect2I(cutX, 0, w - cutX, cutY), TRANSPARENT);
        }
        else
        {
            FillBottomRightCorner(mask, cutX + 1, cutY - 1, TRANSPARENT);
        }
    }

    private static void StoreLeftMask(IGraphicAssetCollection collection, Image mask, int size, int leftCut, int rightCut)
    {

        string name = "mask_" + size + "_0_" + GetBorderType(leftCut, rightCut);
        collection.AddAsset(name, mask, false);

        name = "mask_" + size + "_3_" + GetBorderType(rightCut, leftCut);
        collection.AddAsset(name, FlipV(mask), false);

        name = "mask_" + size + "_4_" + GetBorderType(leftCut, rightCut);
        collection.AddAsset(name, FlipHV(mask), false);

        name = "mask_" + size + "_7_" + GetBorderType(rightCut, leftCut);
        collection.AddAsset(name, FlipH(mask), false);
    }

    private static void StoreRightMask(IGraphicAssetCollection collection, Image mask, int size, int leftCut, int rightCut)
    {

        string name = "mask_" + size + "_1_" + GetBorderType(leftCut, rightCut);
        collection.AddAsset(name, mask, false);

        name = "mask_" + size + "_2_" + GetBorderType(rightCut, leftCut);
        collection.AddAsset(name, FlipV(mask), false);

        name = "mask_" + size + "_5_" + GetBorderType(leftCut, rightCut);
        collection.AddAsset(name, FlipHV(mask), false);

        name = "mask_" + size + "_6_" + GetBorderType(rightCut, leftCut);
        collection.AddAsset(name, FlipH(mask), false);
    }

    private static void FillTopLeftCorner(Image image, int startX, int startY, int step, Color color)
    {
        int x = startX;
        int y = startY;
        int counter = step;
        int w = image.GetWidth();
        int h = image.GetHeight();

        while (y >= 0)
        {
            for (int py = y; py >= 0; py--)
            {
                if (x >= 0 && x < w && py >= 0 && py < h)
                {
                    image.SetPixel(x, py, color);
                }
            }

            counter++;

            if (counter >= 2)
            {
                y--;
                counter = 0;
            }

            x++;
        }
    }

    private static void FillBottomRightCorner(Image image, int startX, int startY, Color color)
    {
        int x = startX;
        int y = startY;
        int w = image.GetWidth();

        while (x < w)
        {
            for (int px = x; px < w; px++)
            {
                if (px >= 0 && y >= 0 && y < image.GetHeight())
                {
                    image.SetPixel(px, y, color);
                }
            }

            y--;
            x += 2;
        }
    }

    private static Image FlipH(Image source)
    {
        Image copy = (Image)source.Duplicate();

        copy.FlipX();

        return copy;
    }

    private static Image FlipV(Image source)
    {
        Image copy = (Image)source.Duplicate();

        copy.FlipY();

        return copy;
    }

    private static Image FlipHV(Image source)
    {
        Image copy = (Image)source.Duplicate();

        copy.FlipX();
        copy.FlipY();

        return copy;
    }

    private static void CopyPixelsWithMask(Image dest, Image mask)
    {
        int w = Math.Min(dest.GetWidth(), mask.GetWidth());
        int h = Math.Min(dest.GetHeight(), mask.GetHeight());

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color maskPixel = mask.GetPixel(x, y);

                if (maskPixel.A > 0)
                {
                    dest.SetPixel(x, y, maskPixel);
                }
            }
        }
    }
}
