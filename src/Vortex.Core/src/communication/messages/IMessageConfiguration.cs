using System;

namespace Vortex.Core.Communication.Messages;

public interface IMessageConfiguration
{
    Dictionary<int, Type> events { get; }
    Dictionary<int, Type> composers { get; }
}
