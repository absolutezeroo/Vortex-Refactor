using System;

using Godot.Collections;

using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Messages;

namespace Vortex.Core.Communication.Connection;

public interface IConnection
{
    bool disposed { get; }

    bool connected { get; }

    int timeout { set; }

    void Dispose();

    bool Init(string param1, uint param2 = 0, bool param3 = true);

    bool Send(IMessageComposer param1);

    bool SendUnencrypted(IMessageComposer param1);

    void SetEncryption(IEncryption param1, IEncryption? param2);

    IEncryption? GetServerToClientEncryption();

    void RegisterMessageClasses(IMessageConfiguration param1);

    void AddMessageEvent(IMessageEvent param1);

    void RemoveMessageEvent(IMessageEvent param1);

    void ProcessReceivedData();

    void Close();

    void IsAuthenticated();

    void IsConfigured();

    void CreateSocket();

    void AddListener(string param1, Action<Dictionary> param2);
}
