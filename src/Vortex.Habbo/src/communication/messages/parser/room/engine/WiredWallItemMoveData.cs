namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1676
public class WiredWallItemMoveData(
    int itemId, bool isDirectionRight,
    int oldWallX, int oldWallY, int oldOffsetX, int oldOffsetY,
    int newWallX, int newWallY, int newOffsetX, int newOffsetY,
    double animationTime)
{
    public int ItemId => itemId;
    public bool IsDirectionRight => isDirectionRight;
    public int OldWallX => oldWallX;
    public int OldWallY => oldWallY;
    public int OldOffsetX => oldOffsetX;
    public int OldOffsetY => oldOffsetY;
    public int NewWallX => newWallX;
    public int NewWallY => newWallY;
    public int NewOffsetX => newOffsetX;
    public int NewOffsetY => newOffsetY;
    public double AnimationTime => animationTime;
}
