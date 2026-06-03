namespace Vortex.Room.Utils;

/// <summary>
/// Animates RGB color transitions over time with HSL-based alpha blending.
/// </summary>
/// @see com.sulake.room.utils.ColorTransitioner
public class ColorTransitioner(uint color = 0xFFFFFF, int alpha = 255)
{
    private uint _color = color;
    private int _alpha = alpha;
    private uint _originalColor = color;
    private int _originalAlpha = alpha;
    private uint _targetColor = color;
    private int _targetAlpha = alpha;
    private int _colorChangedTime;
    private int _colorTransitionLength;

    public void StartTransition(int targetColor, int targetAlpha, int currentTime, int duration = 1500)
    {
        _originalColor = _color;
        _originalAlpha = _alpha;
        _targetColor = (uint)targetColor;
        _targetAlpha = targetAlpha;
        _colorChangedTime = currentTime;
        _colorTransitionLength = duration;
    }

    public bool UpdateColor(int currentTime)
    {
        if (_colorChangedTime == 0)
        {
            return false;
        }

        if (currentTime - _colorChangedTime >= _colorTransitionLength)
        {
            _color = _targetColor;
            _alpha = _targetAlpha;
            _colorChangedTime = 0;
        }
        else
        {
            int origR = (int)((_originalColor >> 16) & 0xFF);
            int origG = (int)((_originalColor >> 8) & 0xFF);
            int origB = (int)(_originalColor & 0xFF);
            int targR = (int)((_targetColor >> 16) & 0xFF);
            int targG = (int)((_targetColor >> 8) & 0xFF);
            int targB = (int)(_targetColor & 0xFF);

            double t = (double)(currentTime - _colorChangedTime) / _colorTransitionLength;
            int r = origR + (int)((targR - origR) * t);
            int g = origG + (int)((targG - origG) * t);
            int b = origB + (int)((targB - origB) * t);

            _color = (uint)((r << 16) + (g << 8) + b);
            _alpha = (int)(_originalAlpha + ((_targetAlpha - _originalAlpha) * t));
        }
        return true;
    }

    public int Color
    {
        get
        {
            int hsl = ColorConverter.RgbToHSL((int)_color);
            hsl = (hsl & 0xFFFF00) + _alpha;

            return ColorConverter.HslToRGB(hsl);
        }
    }
}
