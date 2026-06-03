using System;

namespace Vortex.Room.Exceptions;

/// @see com.sulake.room.exceptions.RoomManagerException
public class RoomManagerException(string message = "", int id = 0) : Exception(message)
{
    public int Id { get; } = id;
}
