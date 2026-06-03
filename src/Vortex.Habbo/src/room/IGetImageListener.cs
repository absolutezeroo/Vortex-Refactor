// @see habbo/room/IGetImageListener.as

using Godot;

namespace Vortex.Habbo.Room;

/// @see habbo/room/IGetImageListener.as
public interface IGetImageListener
{
    void ImageReady(int param1, Image? param2);
    void ImageFailed(int param1);
}
