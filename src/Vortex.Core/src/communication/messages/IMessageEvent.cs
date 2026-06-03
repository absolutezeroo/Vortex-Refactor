using System;

using Vortex.Core.Communication.Connection;

namespace Vortex.Core.Communication.Messages;

public interface IMessageEvent
{
    Action<IMessageEvent>? callback { get; }

    IConnection? connection { get; set; }

    Type parserClass { get; }

    IMessageParser? parser { get; set; }

    void Dispose();
}
