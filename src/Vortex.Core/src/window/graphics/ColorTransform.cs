// @see flash.geom.ColorTransform (AS3 built-in)

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// C# equivalent of flash.geom.ColorTransform.
/// </summary>
public class ColorTransform(float redMultiplier = 1.0f,
    float greenMultiplier = 1.0f,
    float blueMultiplier = 1.0f,
    float alphaMultiplier = 1.0f,
    float redOffset = 0.0f,
    float greenOffset = 0.0f,
    float blueOffset = 0.0f,
    float alphaOffset = 0.0f)
{
    public float RedMultiplier { get; set; } = redMultiplier;
    public float GreenMultiplier { get; set; } = greenMultiplier;
    public float BlueMultiplier { get; set; } = blueMultiplier;
    public float AlphaMultiplier { get; set; } = alphaMultiplier;
    public float RedOffset { get; set; } = redOffset;
    public float GreenOffset { get; set; } = greenOffset;
    public float BlueOffset { get; set; } = blueOffset;
    public float AlphaOffset { get; set; } = alphaOffset;

    /// @see flash.geom.ColorTransform::concat
    public void Concat(ColorTransform other)
    {
        RedMultiplier *= other.RedMultiplier;
        GreenMultiplier *= other.GreenMultiplier;
        BlueMultiplier *= other.BlueMultiplier;
        AlphaMultiplier *= other.AlphaMultiplier;
        RedOffset = (RedOffset * other.RedMultiplier) + other.RedOffset;
        GreenOffset = (GreenOffset * other.GreenMultiplier) + other.GreenOffset;
        BlueOffset = (BlueOffset * other.BlueMultiplier) + other.BlueOffset;
        AlphaOffset = (AlphaOffset * other.AlphaMultiplier) + other.AlphaOffset;
    }

    /// @see flash.geom.ColorTransform::get color
    public uint Color
    {
        get
        {
            uint r = (uint)System.Math.Clamp(RedOffset, 0, 255);
            uint g = (uint)System.Math.Clamp(GreenOffset, 0, 255);
            uint b = (uint)System.Math.Clamp(BlueOffset, 0, 255);
            return (r << 16) | (g << 8) | b;
        }
        set
        {
            RedMultiplier = 0;
            GreenMultiplier = 0;
            BlueMultiplier = 0;
            RedOffset = (value >> 16) & 0xFF;
            GreenOffset = (value >> 8) & 0xFF;
            BlueOffset = value & 0xFF;
        }
    }

    public override string ToString()
    {
        return
            $"(redMultiplier={RedMultiplier}, greenMultiplier={GreenMultiplier}, blueMultiplier={BlueMultiplier}, alphaMultiplier={AlphaMultiplier}, redOffset={RedOffset}, greenOffset={GreenOffset}, blueOffset={BlueOffset}, alphaOffset={AlphaOffset})";
    }
}
