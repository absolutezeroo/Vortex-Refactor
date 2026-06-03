using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Communication.Wireformat;
using Vortex.Core.Utils;

using Array = System.Array;

namespace Vortex.Core.Communication.Connection;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as
public partial class SocketConnection : RefCounted, IConnection, IDisposable
{
    private const int DEFAULT_SOCKET_TIMEOUT = 10000;

    private readonly System.Collections.Generic.Dictionary<string, List<Action<Dictionary>>> _listeners = new();
    private readonly MessageClassManager _messageClassManager;
    private readonly List<IMessageComposer> _pendingClientMessages = [];
    private readonly List<IMessageDataWrapper> _pendingServerMessages = [];
    private readonly IWireFormat _wireFormat;
    private ICoreCommunicationManager? _communicationManager;
    private bool _configurationReady;
    private IMessageDataWrapper? _lastProcessedMessage;
    private int _timeout = DEFAULT_SOCKET_TIMEOUT;
    private TcpClient? _socket;
    private byte[] _dataBuffer = [];
    private IConnectionStateListener? _stateListener;
    private int var_2924;
    private IEncryption? _clientToServerEncryption;
    private IEncryption? _serverToClientEncryption;
    private bool var_4186;

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::class_3349
    public SocketConnection(ICoreCommunicationManager param1, IConnectionStateListener? param2)
    {
        _communicationManager = param1;

        _messageClassManager = new MessageClassManager();
        _wireFormat = new EvaWireFormat();

        CreateSocket();

        _stateListener = param2;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::disposed
    public bool disposed { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::connected
    public bool connected => _socket is { Connected: true };

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::addListener
    public void AddListener(string param1, Action<Dictionary> param2)
    {
        if (!_listeners.TryGetValue(param1, out List<Action<Dictionary>>? list))
        {
            list = [];

            _listeners[param1] = list;
        }

        list.Add(param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::dispose
    public new void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        DisposeSocket();

        _dataBuffer = [];
        _stateListener = null;
        _clientToServerEncryption = null;
        _serverToClientEncryption = null;
        _wireFormat.Dispose();
        _messageClassManager.Dispose();
        _communicationManager = null;
        _lastProcessedMessage = null;
        _listeners.Clear();

        GC.SuppressFinalize(this);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::createSocket
    public void CreateSocket()
    {
        DisposeSocket();

        _dataBuffer = [];
        _serverToClientEncryption = null;
        _clientToServerEncryption = null;
        _socket = new TcpClient();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::init
    public bool Init(string param1, uint param2 = 0, bool param3 = true)
    {
        if (disposed || _socket == null)
        {
            return false;
        }

        _stateListener?.ConnectionInit(param1, (int)param2);

        var_2924 = (int)Time.GetTicksMsec();

        try
        {
            _socket.NoDelay = param3;
            _socket.SendTimeout = _timeout;
            _socket.ReceiveTimeout = _timeout;

            Task connectTask = _socket.ConnectAsync(param1, (int)param2);

            if (!connectTask.Wait(_timeout))
            {
                int timeoutElapsed = (int)Time.GetTicksMsec() - var_2924;

                ErrorReportStorage.AddDebugData("ConnectionTimer", "TimeOut in " + timeoutElapsed);
                Logger.Warn($"[SocketConnection] Connection timeout after {_timeout}ms");

                Dispatch("ioError", "Socket connection timeout");

                return false;
            }

            int connectElapsed = (int)Time.GetTicksMsec() - var_2924;

            ErrorReportStorage.AddDebugData("ConnectionTimer", "Connected in " + connectElapsed);

            Dispatch("connect", "");

            return true;
        }
        catch (Exception e)
        {
            int errorElapsed = (int)Time.GetTicksMsec() - var_2924;
            ErrorReportStorage.AddDebugData("ConnectionTimer", "IOError in " + errorElapsed);
            Logger.Error($"[SocketConnection] Connection failed: {e.Message}", e);
            Dispatch("ioError", e.Message);

            return false;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::timeout
    public int timeout
    {
        set => _timeout = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::addMessageEvent
    public void AddMessageEvent(IMessageEvent param1)
    {
        if (disposed)
        {
            return;
        }

        _messageClassManager.RegisterMessageEvent(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::removeMessageEvent
    public void RemoveMessageEvent(IMessageEvent param1)
    {
        if (disposed)
        {
            return;
        }

        _messageClassManager.UnregisterMessageEvent(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::isAuthenticated
    public void IsAuthenticated()
    {
        var_4186 = true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::isConfigured
    public void IsConfigured()
    {
        _configurationReady = true;

        foreach (IMessageDataWrapper message in _pendingServerMessages)
        {
            int id = message.GetId();
            List<IMessageEvent> events = ParseReceivedMessage(message);

            if (events.Count > 0)
            {
                HandleReceivedMessage(id, events);
            }
        }

        foreach (IMessageComposer composer in _pendingClientMessages)
        {
            Send(composer);
        }

        _pendingClientMessages.Clear();
        _pendingServerMessages.Clear();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::send
    public bool Send(IMessageComposer composer)
    {
        if (disposed)
        {
            return false;
        }

        if (var_4186 && !_configurationReady)
        {
            _pendingClientMessages.Add(composer);

            return false;
        }

        int messageId = _messageClassManager.GetMessageIdForComposer(composer);

        if (messageId < 0)
        {
            return false;
        }

        byte[] encoded = _wireFormat.Encode(messageId, composer.GetMessageArray());

        _stateListener?.MessageSent(messageId.ToString());

        if (_socket is not { Connected: true })
        {
            return false;
        }

        if (_clientToServerEncryption == null)
        {
            return false;
        }

        _clientToServerEncryption.Encipher(encoded);

        return Write(encoded);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::sendUnencrypted
    public bool SendUnencrypted(IMessageComposer composer)
    {
        if (disposed)
        {
            return false;
        }

        if (composer is not IPreEncryptionMessage)
        {
            return false;
        }

        int messageId = _messageClassManager.GetMessageIdForComposer(composer);

        if (messageId < 0)
        {
            return false;
        }

        byte[] encoded = _wireFormat.Encode(messageId, composer.GetMessageArray());

        _stateListener?.MessageSent(messageId.ToString());

        return _socket is { Connected: true } && Write(encoded);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::setEncryption
    public void SetEncryption(IEncryption param1, IEncryption? param2)
    {
        _clientToServerEncryption = param1;
        _serverToClientEncryption = param2;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::getServerToClientEncryption
    public IEncryption? GetServerToClientEncryption()
    {
        return _serverToClientEncryption;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::registerMessageClasses
    public void RegisterMessageClasses(IMessageConfiguration param1)
    {
        _messageClassManager.RegisterMessages(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::processReceivedData
    public void ProcessReceivedData()
    {
        if (disposed || _socket is not { Connected: true })
        {
            return;
        }

        try
        {
            if (_socket.Client.Poll(0, SelectMode.SelectRead) && _socket.Available == 0)
            {
                Close();

                return;
            }

            NetworkStream stream = _socket.GetStream();

            if (_socket.Available > 0)
            {
                byte[] chunk = new byte[_socket.Available];
                int read = stream.Read(chunk, 0, chunk.Length);

                if (read > 0)
                {
                    if (read != chunk.Length)
                    {
                        byte[] resize = new byte[read];

                        Array.Copy(chunk, resize, read);

                        chunk = resize;
                    }

                    _dataBuffer = Append(_dataBuffer, chunk);
                }
            }

            ProcessData();
        }
        catch (Exception e)
        {
            if (_lastProcessedMessage != null)
            {
                _stateListener?.MessageParseError(_lastProcessedMessage);
            }

            Dispatch("ioError", e.Message);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_3349.as::close
    public void Close()
    {
        if (_socket == null)
        {
            return;
        }

        int closeElapsed = (int)Time.GetTicksMsec() - var_2924;

        ErrorReportStorage.AddDebugData("ConnectionTimer", "Closed in " + closeElapsed);

        try
        {
            if (_socket.Connected)
            {
                _socket.Close();
            }
        }
        catch
        {
            // ignored
        }

        Dispatch("close", "");
    }

    private void DisposeSocket()
    {
        if (_socket == null)
        {
            return;
        }
        try
        {
            if (_socket.Connected)
            {
                _socket.Close();
            }
        }
        catch
        {
            // ignored
        }

        _socket.Dispose();
        _socket = null;
    }

    private bool Write(byte[] encoded)
    {
        try
        {
            NetworkStream stream = _socket!.GetStream();

            stream.Write(encoded, 0, encoded.Length);
            stream.Flush();

            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"[SocketConnection] Write failed: {e.Message}", e);

            Dispatch("ioError", e.Message);

            return false;
        }
    }

    private void ProcessData()
    {
        List<IMessageDataWrapper> wrappers = SplitReceivedMessages();

        foreach (IMessageDataWrapper wrapper in wrappers)
        {
            _lastProcessedMessage = wrapper;

            int id = wrapper.GetId();

            _stateListener?.MessageReceived(id.ToString());

            if (var_4186 && !_configurationReady)
            {
                _pendingServerMessages.Add(wrapper);
            }
            else
            {
                List<IMessageEvent> events = ParseReceivedMessage(wrapper);

                if (events.Count > 0)
                {
                    HandleReceivedMessage(id, events);
                }
            }
        }
    }

    private List<IMessageDataWrapper> SplitReceivedMessages()
    {
        if (_dataBuffer.Length == 0)
        {
            return [];
        }

        List<IMessageDataWrapper> list = _wireFormat.SplitMessages(_dataBuffer, this);

        _dataBuffer = _wireFormat.GetRemainder();

        return list;
    }

    private List<IMessageEvent> ParseReceivedMessage(IMessageDataWrapper param1)
    {
        List<IMessageEvent> events = _messageClassManager.GetMessageEventsForId(param1.GetId());

        if (events.Count == 0)
        {
            return events;
        }

        IMessageParser? parser = events[0].parser;

        parser?.Flush();
        parser?.Parse(param1);

        return events;
    }

    private void HandleReceivedMessage(int param1, List<IMessageEvent> param2)
    {
        foreach (IMessageEvent e in param2)
        {
            e.connection = this;
            e.callback?.Invoke(e);
        }
    }

    private static byte[] Append(byte[] left, byte[] right)
    {
        if (left.Length == 0)
        {
            return right;
        }

        if (right.Length == 0)
        {
            return left;
        }

        byte[] result = new byte[left.Length + right.Length];

        Array.Copy(left, 0, result, 0, left.Length);
        Array.Copy(right, 0, result, left.Length, right.Length);

        return result;
    }

    public void Dispatch(string eventType, string text)
    {
        if (!_listeners.TryGetValue(eventType, out List<Action<Dictionary>>? list))
        {
            return;
        }

        Dictionary payload = new()
        {
            ["type"] = eventType,
            ["text"] = text,
        };

        foreach (Action<Dictionary> callback in list)
        {
            callback(payload);
        }
    }
}
