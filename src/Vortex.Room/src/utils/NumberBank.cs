namespace Vortex.Room.Utils;

/// <summary>
/// Reusable integer ID pool.
/// </summary>
/// @see com.sulake.room.utils.NumberBank
public class NumberBank
{
    private List<int>? _reservedNumbers;
    private List<int>? _freeNumbers;

    public NumberBank(int count)
    {
        _reservedNumbers = new List<int>();
        _freeNumbers = new List<int>();

        if (count < 0)
        {
            count = 0;
        }

        for (int i = 0; i < count; i++)
        {
            _freeNumbers.Add(i);
        }
    }

    public void Dispose()
    {
        _reservedNumbers = null;
        _freeNumbers = null;
    }

    public int ReserveNumber()
    {
        if (_freeNumbers is not { Count: > 0 })
        {
            return -1;
        }

        int number = _freeNumbers[^1];
        _freeNumbers.RemoveAt(_freeNumbers.Count - 1);
        _reservedNumbers?.Add(number);

        return number;

    }

    public void FreeNumber(int number)
    {
        if (_reservedNumbers == null || _freeNumbers == null)
        {
            return;
        }

        int index = _reservedNumbers.IndexOf(number);

        if (index < 0)
        {
            return;
        }

        _reservedNumbers.RemoveAt(index);
        _freeNumbers.Add(number);
    }
}
