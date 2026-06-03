namespace Vortex.Core.Communication.Util;

public class Short
{
    public Short(int param1)
    {
        int v = param1 & 0xFFFF;

        if (v >= 0x8000)
        {
            v -= 0x10000;
        }

        value = v;
    }

    public int value { get; }
}
