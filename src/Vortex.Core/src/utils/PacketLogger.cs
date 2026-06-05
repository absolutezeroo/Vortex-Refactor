using Godot;

namespace Vortex.Core.Utils;

/// <summary>
/// Dev-only packet logger. Set <see cref="Enabled"/> to true at runtime to print
/// every incoming/outgoing message ID and type name to the Godot output.
/// Not part of the AS3 source — pure development utility.
/// </summary>
public static class PacketLogger
{
    public static bool Enabled { get; set; } = false;

    public static void LogIncoming(int id, string typeName)
        => GD.Print($"[PACKET] ← IN  [{id,5}] {typeName}");

    public static void LogOutgoing(int id, string typeName)
        => GD.Print($"[PACKET] → OUT [{id,5}] {typeName}");

    public static void LogUnknownIncoming(int id)
        => GD.PrintErr($"[PACKET] ← IN  [{id,5}] ??? (not registered)");

    public static void LogParseError(int id, string typeName)
        => GD.PrintErr($"[PACKET] ← ERR [{id,5}] {typeName} — parse failed");
}
