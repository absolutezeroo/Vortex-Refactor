using System;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Utils;

/// @see com.sulake.habbo.room.object.visualization.room.utils.Randomizer
public class Randomizer
{
    public const int DEFAULT_SEED = 1;
    public const int DEFAULT_MODULUS = 16777216;

    private static Randomizer? _instance;

    private int _seed = 1;
    private readonly int _multiplier = 69069;
    private readonly int _increment = 5;
    private int _modulus = DEFAULT_MODULUS;

    public static void SetSeed(int seed = DEFAULT_SEED)
    {
        _instance ??= new Randomizer();
        _instance._seed = seed;
    }

    public static void SetModulus(int modulus = DEFAULT_MODULUS)
    {
        _instance ??= new Randomizer();

        if (modulus < 1)
        {
            modulus = 1;
        }

        _instance._modulus = modulus;
    }

    public static int[] GetValues(int count, int min, int max)
    {
        _instance ??= new Randomizer();

        return _instance.GetRandomValues(count, min, max);
    }

    public static int[]? GetArray(int count, int max)
    {
        _instance ??= new Randomizer();

        return _instance.GetRandomArray(count, max);
    }

    public int[] GetRandomValues(int count, int min, int range)
    {
        int[] result = new int[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = IterateScaled(min, range - min);
        }

        return result;
    }

    public int[]? GetRandomArray(int count, int max)
    {
        if (count > max || max > 1000)
        {
            return null;
        }

        List<int> pool = new();

        for (int i = 0; i <= max; i++)
        {
            pool.Add(i);
        }

        int[] result = new int[count];

        for (int i = 0; i < count; i++)
        {
            int index = IterateScaled(0, pool.Count - 1);

            result[i] = pool[index];
            pool.RemoveAt(index);
        }

        return result;
    }

    private int Iterate()
    {
        int value = _multiplier * _seed + _increment;

        if (value < 0)
        {
            value = -value;
        }

        value %= _modulus;
        _seed = value;

        return value;
    }

    private int IterateScaled(int min, int range)
    {
        int value = Iterate();

        if (range < 1)
        {
            return min;
        }

        return (int)(min + (double)value / _modulus * range);
    }
}
