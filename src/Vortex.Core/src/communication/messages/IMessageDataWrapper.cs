namespace Vortex.Core.Communication.Messages;

public interface IMessageDataWrapper
{
    uint bytesAvailable { get; }
    int GetId();

    string ReadString();

    int ReadInteger();

    double ReadLong();

    bool ReadBoolean();

    int ReadShort();

    int ReadByte();

    float ReadFloat();

    double ReadDouble();
}
