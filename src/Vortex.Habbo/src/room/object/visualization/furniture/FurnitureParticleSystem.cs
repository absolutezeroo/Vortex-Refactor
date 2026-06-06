using System;
using System.Globalization;
using System.Xml.Linq;

using Godot;

using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureParticleSystem
public class FurnitureParticleSystem(AnimatedFurnitureVisualization visualization)
{
    private Dictionary<int, FurnitureParticleSystemEmitter> _emitters = new();
    private int _systemSize;
    private int _canvasSpriteId = -1;
    private int _offsetY;
    private FurnitureParticleSystemEmitter? _activeEmitter;
    private Image? _canvas;
    private IRoomObjectSprite? _canvasSprite;
    private bool _isIgnited;
    private int _canvasOffsetX;
    private int _canvasOffsetY;
    private double _scale = 1;
    private double _blendAlpha = 1;
    private uint _bgColor = 0xFF000000;

    public void Dispose()
    {
        foreach (FurnitureParticleSystemEmitter emitter in _emitters.Values)
        {
            emitter.Dispose();
        }

        _emitters = null!;
        _canvas = null;
    }

    public void Reset()
    {
        _activeEmitter?.Reset();
        _activeEmitter = null;
        _isIgnited = false;
        UpdateCanvas();
    }

    public void SetAnimation(int animationId)
    {
        _activeEmitter?.Reset();
        _emitters.TryGetValue(animationId, out _activeEmitter);
        _isIgnited = false;
        UpdateCanvas();
    }

    public int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if (_activeEmitter != null && _activeEmitter.RoomObjectSpriteId == layer)
        {
            return (int)(_activeEmitter.Y * _scale);
        }

        return 0;
    }

    public bool ControlsSprite(int spriteIndex)
    {
        return _activeEmitter?.RoomObjectSpriteId == spriteIndex;
    }

    public void UpdateSprites()
    {
        if (_activeEmitter == null || _canvasSprite == null)
        {
            return;
        }

        if (_canvas != null && _canvasSprite.Asset != _canvas)
        {
            _canvasSprite.Asset = _canvas;
        }

        if (_isIgnited && _activeEmitter.RoomObjectSpriteId >= 0)
        {
            IRoomObjectSprite? emitterSprite = visualization.GetSprite(_activeEmitter.RoomObjectSpriteId);

            if (emitterSprite != null)
            {
                emitterSprite.Visible = false;
            }
        }
    }

    public void UpdateAnimation()
    {
        if (_activeEmitter == null || _canvasSprite == null)
        {
            return;
        }

        if (!_isIgnited && _activeEmitter.HasIgnited)
        {
            _isIgnited = true;
        }

        int offsetYScaled = (int)(_offsetY * _scale);
        _activeEmitter.Update();

        if (_isIgnited)
        {
            if (_activeEmitter.RoomObjectSpriteId >= 0)
            {
                IRoomObjectSprite? emitterSprite = visualization.GetSprite(_activeEmitter.RoomObjectSpriteId);

                if (emitterSprite != null)
                {
                    emitterSprite.Visible = false;
                }
            }

            if (_canvas == null)
            {
                UpdateCanvas();
            }

            if (_canvas == null)
            {
                return;
            }

            int width = _canvas.GetWidth();
            int height = _canvas.GetHeight();

            FillCanvasBackground(width, height);

            double projScale = 10.0;

            foreach (FurnitureParticleSystemParticle particle in _activeEmitter.Particles)
            {
                double py = particle.Y;
                int screenX = _canvasOffsetX + (int)((particle.X - particle.Z) * projScale / 10.0 * _scale);
                int screenY = _canvasOffsetY - offsetYScaled + (int)((py + ((particle.X + particle.Z) / 2.0)) * projScale / 10.0 * _scale);

                IGraphicAsset? asset = particle.GetAsset();

                if (asset?.Asset?.Content is Image assetImage)
                {
                    int assetW = assetImage.GetWidth();
                    int assetH = assetImage.GetHeight();
                    int destX = screenX + asset.OffsetX;
                    int destY = screenY + asset.OffsetY;

                    if (particle.Fade && particle.AlphaMultiplier < 1)
                    {
                        float alpha = (float)particle.AlphaMultiplier;
                        BlitWithAlpha(_canvas, assetImage, destX, destY, assetW, assetH, width, height, alpha);
                    }
                    else
                    {
                        BlitRect(_canvas, assetImage, destX, destY, assetW, assetH, width, height);
                    }
                }
                else
                {
                    FillRect(_canvas, screenX - 1, screenY - 1, 2, 2, width, height);
                }
            }
        }
    }

    public void ParseData(XElement xml)
    {
        _systemSize = int.Parse(xml.Attribute("size")?.Value ?? "64", CultureInfo.InvariantCulture);
        _canvasSpriteId = int.Parse(xml.Attribute("canvas_id")?.Value ?? "-1", CultureInfo.InvariantCulture);
        _offsetY = int.Parse(xml.Attribute("offset_y")?.Value ?? "10", CultureInfo.InvariantCulture);
        _scale = _systemSize / 64.0;

        _blendAlpha = double.Parse(xml.Attribute("blend")?.Value ?? "1", CultureInfo.InvariantCulture);
        _blendAlpha = Math.Min(_blendAlpha, 1);

        string bgStr = xml.Attribute("bgcolor")?.Value ?? "0";
        _bgColor = xml.Attribute("bgcolor") != null ? uint.Parse(bgStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture) : 0xFF000000;

        foreach (XElement emitterElement in xml.Elements("emitter"))
        {
            int id = int.Parse(emitterElement.Attribute("id")?.Value ?? "0", CultureInfo.InvariantCulture);
            string name = emitterElement.Attribute("name")?.Value ?? "";
            int spriteId = int.Parse(emitterElement.Attribute("sprite_id")?.Value ?? "-1", CultureInfo.InvariantCulture);

            FurnitureParticleSystemEmitter emitter = new(name, spriteId);
            _emitters[id] = emitter;

            int maxParticles = int.Parse(emitterElement.Attribute("max_num_particles")?.Value ?? "0", CultureInfo.InvariantCulture);
            int particlesPerFrame = int.Parse(emitterElement.Attribute("particles_per_frame")?.Value ?? "0", CultureInfo.InvariantCulture);
            int burstPulse = int.Parse(emitterElement.Attribute("burst_pulse")?.Value ?? "1", CultureInfo.InvariantCulture);
            int fuseTime = int.Parse(emitterElement.Attribute("fuse_time")?.Value ?? "10", CultureInfo.InvariantCulture);

            XElement? sim = emitterElement.Element("simulation");
            double force = double.Parse(sim?.Attribute("force")?.Value ?? "0", CultureInfo.InvariantCulture);
            double direction = double.Parse(sim?.Attribute("direction")?.Value ?? "0", CultureInfo.InvariantCulture);
            double gravity = double.Parse(sim?.Attribute("gravity")?.Value ?? "0", CultureInfo.InvariantCulture);
            double airFriction = double.Parse(sim?.Attribute("airfriction")?.Value ?? "0", CultureInfo.InvariantCulture);
            string shape = sim?.Attribute("shape")?.Value ?? FurnitureParticleSystemEmitter.SHAPE_CONE;
            double energy = double.Parse(sim?.Attribute("energy")?.Value ?? "1", CultureInfo.InvariantCulture);

            XElement? particlesElement = emitterElement.Element("particles");

            if (particlesElement != null)
            {
                foreach (XElement particleElement in particlesElement.Elements("particle"))
                {
                    int lifeTime = int.Parse(particleElement.Attribute("lifetime")?.Value ?? "20", CultureInfo.InvariantCulture);
                    bool isEmitter = particleElement.Attribute("is_emitter")?.Value != "false";
                    bool fade = particleElement.Attribute("fade")?.Value == "true";

                    List<IGraphicAsset> frames = new();

                    foreach (XElement frameElement in particleElement.Elements("frame"))
                    {
                        string frameName = frameElement.Attribute("name")?.Value ?? "";
                        IGraphicAsset? asset = visualization.AssetCollection?.GetAsset(frameName);

                        if (asset != null)
                        {
                            frames.Add(asset);
                        }
                    }

                    emitter.ConfigureParticle(lifeTime, isEmitter, frames.ToArray(), fade);
                }
            }

            emitter.Setup(maxParticles, particlesPerFrame, force, 0, direction, 0, gravity, airFriction, shape, energy, fuseTime, burstPulse);
        }
    }

    public void CopyStateFrom(FurnitureParticleSystem? other)
    {
        if (other == null)
        {
            return;
        }

        int activeKey = 0;

        if (other._activeEmitter != null)
        {
            foreach (KeyValuePair<int, FurnitureParticleSystemEmitter> kvp in other._emitters)
            {
                if (kvp.Value == other._activeEmitter)
                {
                    activeKey = kvp.Key;
                    break;
                }
            }
        }

        SetAnimation(activeKey);

        if (_activeEmitter != null && other._activeEmitter != null)
        {
            double scaleFactor = other._systemSize / (double)_systemSize;
            _activeEmitter.CopyStateFrom(other._activeEmitter, scaleFactor);
        }

        _canvas = null;
    }

    private void UpdateCanvas()
    {
        if (_activeEmitter == null)
        {
            return;
        }

        if (_canvasSpriteId >= 0)
        {
            _canvasSprite = visualization.GetSprite(_canvasSpriteId);

            if (_canvasSprite?.Asset != null)
            {
                int spriteW = _canvasSprite.Width;
                int spriteH = _canvasSprite.Height;

                if (spriteW <= 1 || spriteH <= 1)
                {
                    return;
                }

                if (_canvas != null && (_canvas.GetWidth() != spriteW || _canvas.GetHeight() != spriteH))
                {
                    _canvas = null;
                }

                if (_canvas == null)
                {
                    _canvas = Image.CreateEmpty(spriteW, spriteH, false, Image.Format.Rgba8);
                }

                _canvasOffsetX = -_canvasSprite.OffsetX;
                _canvasOffsetY = -_canvasSprite.OffsetY;
                _canvasSprite.Asset = _canvas;
            }

            if (_canvas != null)
            {
                FillCanvasBackground(_canvas.GetWidth(), _canvas.GetHeight());
            }
        }
    }

    private void FillCanvasBackground(int width, int height)
    {
        if (_canvas == null)
        {
            return;
        }

        float a = ((_bgColor >> 24) & 0xFF) / 255f;
        float r = ((_bgColor >> 16) & 0xFF) / 255f;
        float g = ((_bgColor >> 8) & 0xFF) / 255f;
        float b = (_bgColor & 0xFF) / 255f;

        _canvas.Fill(new Color(r, g, b, a));
    }

    private static void BlitRect(Image canvas, Image src, int destX, int destY, int srcW, int srcH, int canvasW, int canvasH)
    {
        int sx = Math.Max(0, -destX);
        int sy = Math.Max(0, -destY);
        int ex = Math.Min(srcW, canvasW - destX);
        int ey = Math.Min(srcH, canvasH - destY);

        if (sx < ex && sy < ey)
        {
            canvas.BlitRect(src, new Rect2I(sx, sy, ex - sx, ey - sy), new Vector2I(destX + sx, destY + sy));
        }
    }

    private static void BlitWithAlpha(Image canvas, Image src, int destX, int destY, int srcW, int srcH, int canvasW, int canvasH, float alpha)
    {
        for (int py = 0; py < srcH; py++)
        {
            for (int px = 0; px < srcW; px++)
            {
                int dx = destX + px;
                int dy = destY + py;

                if (dx < 0 || dy < 0 || dx >= canvasW || dy >= canvasH)
                {
                    continue;
                }

                Color pixel = src.GetPixel(px, py);

                if (pixel.A <= 0)
                {
                    continue;
                }

                pixel = new Color(pixel.R, pixel.G, pixel.B, pixel.A * alpha);
                canvas.SetPixel(dx, dy, pixel);
            }
        }
    }

    private static void FillRect(Image canvas, int x, int y, int w, int h, int canvasW, int canvasH)
    {
        for (int py = y; py < y + h; py++)
        {
            for (int px = x; px < x + w; px++)
            {
                if (px >= 0 && py >= 0 && px < canvasW && py < canvasH)
                {
                    canvas.SetPixel(px, py, Colors.White);
                }
            }
        }
    }
}
