// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as

using System;
using System.Linq;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as
internal sealed class InterfaceStructList : IDisposable
{
    private List<InterfaceStruct>? _interfaceStructs;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::InterfaceStructList
    public InterfaceStructList()
    {
        _interfaceStructs = [];
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::get length
    public uint length => (uint)(_interfaceStructs?.Count ?? 0);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::get disposed
    public bool disposed => _interfaceStructs == null;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::dispose
    public void Dispose()
    {
        if (_interfaceStructs == null)
        {
            return;
        }

        foreach (InterfaceStruct @struct in _interfaceStructs)
        {
            @struct.Dispose();
        }

        _interfaceStructs = null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::insert
    public uint Insert(InterfaceStruct param1)
    {
        EnsureNotDisposed();

        _interfaceStructs!.Add(param1);

        return (uint)_interfaceStructs.Count;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::remove
    public InterfaceStruct Remove(uint param1)
    {
        EnsureNotDisposed();
        if (param1 >= _interfaceStructs!.Count)
        {
            throw new InvalidOperationException("Index out of range!");
        }

        InterfaceStruct @struct = _interfaceStructs[(int)param1];

        _interfaceStructs.RemoveAt((int)param1);

        return @struct;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::find
    public IUnknown? Find(IID param1)
    {
        InterfaceStruct? @struct = GetStructByInterface(param1);
        return @struct?.unknown;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getStructByInterface
    public InterfaceStruct? GetStructByInterface(IID param1)
    {
        EnsureNotDisposed();

        string interfaceName = GetIidName(param1);

        return _interfaceStructs!.FirstOrDefault(@struct => string.Equals(@struct.iis, interfaceName, StringComparison.Ordinal));
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getIndexByInterface
    public int GetIndexByInterface(IID param1)
    {
        EnsureNotDisposed();

        string interfaceName = GetIidName(param1);

        for (int i = 0;
             i < _interfaceStructs!.Count;
             i++)
        {
            if (string.Equals(_interfaceStructs[i].iis, interfaceName, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::mapStructsByInterface
    public uint MapStructsByInterface(IID param1, IList<InterfaceStruct> param2)
    {
        EnsureNotDisposed();

        string interfaceName = GetIidName(param1);
        uint count = 0;

        foreach (InterfaceStruct @struct in _interfaceStructs!.Where(@struct => string.Equals(@struct.iis, interfaceName, StringComparison.Ordinal)))
        {
            param2.Add(@struct);
            count++;
        }

        return count;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getStructByImplementor
    public InterfaceStruct? GetStructByImplementor(IUnknown param1)
    {
        EnsureNotDisposed();

        return _interfaceStructs!.FirstOrDefault(@struct => ReferenceEquals(@struct.unknown, param1));
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getIndexByImplementor
    public int GetIndexByImplementor(IUnknown param1)
    {
        EnsureNotDisposed();

        for (int i = 0;
             i < _interfaceStructs!.Count;
             i++)
        {
            if (ReferenceEquals(_interfaceStructs[i].unknown, param1))
            {
                return i;
            }
        }

        return -1;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::mapStructsByImplementor
    public uint MapStructsByImplementor(IUnknown param1, IList<InterfaceStruct> param2)
    {
        EnsureNotDisposed();

        uint count = 0;

        foreach (InterfaceStruct @struct in _interfaceStructs!.Where(@struct => ReferenceEquals(@struct.unknown, param1)))
        {
            param2.Add(@struct);
            count++;
        }

        return count;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getStructByIndex
    public InterfaceStruct? GetStructByIndex(uint param1)
    {
        EnsureNotDisposed();

        return param1 < _interfaceStructs!.Count ? _interfaceStructs[(int)param1] : null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/InterfaceStructList.as::getTotalReferenceCount
    public uint GetTotalReferenceCount()
    {
        EnsureNotDisposed();

        return _interfaceStructs!.Aggregate<InterfaceStruct?, uint>(0, (current, @struct) => current + @struct!.references);
    }

    private static string GetIidName(IID param1)
    {
        return param1.GetType().FullName ?? param1.GetType().Name;
    }

    private void EnsureNotDisposed()
    {
        if (_interfaceStructs == null)
        {
            throw new InvalidOperationException("InterfaceStructList is disposed.");
        }
    }
}
