using System;

using Vortex.Core.Communication.Connection;

namespace Vortex.Core.Communication.Messages;

public class MessageEvent(Action<IMessageEvent> param1, Type param2) : IMessageEvent, IDisposable
{
    public virtual void Dispose()
    {
        callback = null;
        connection = null;
        parser = null;
        parserClass = typeof(object);

        GC.SuppressFinalize(this);
    }

    public Action<IMessageEvent>? callback { get; private set; } = param1;

    public IConnection? connection { get; set; }

    public Type parserClass { get; private set; } = param2;

    public IMessageParser? parser { get; set; }
}
