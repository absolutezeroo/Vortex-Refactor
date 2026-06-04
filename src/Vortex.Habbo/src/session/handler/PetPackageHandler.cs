// @see com.sulake.habbo.session.handler.PetPackageHandler

using Vortex.Core.Communication.Connection;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.PetPackageHandler
public class PetPackageHandler : BaseHandler
{
    /// @see PetPackageHandler.as::PetPackageHandler
    public PetPackageHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
            return;
        // TODO(as3-port): OpenPetPackageRequestedMessageEvent not yet ported
        // TODO(as3-port): OpenPetPackageResultMessageEvent not yet ported
    }

    // TODO(as3-port): onOpenPetPackageRequested — OpenPetPackageRequestedMessageParser (objectId, figureData)
    //   → dispatch RoomSessionPetPackageEvent("RSOPPE_OPEN_PET_PACKAGE_REQUESTED", session, objectId, figureData, 0, null)

    // TODO(as3-port): onOpenPetPackageResult — OpenPetPackageResultMessageParser (objectId, nameValidationStatus, nameValidationInfo)
    //   → dispatch RoomSessionPetPackageEvent("RSOPPE_OPEN_PET_PACKAGE_RESULT", session, objectId, null, nameValidationStatus, nameValidationInfo)
}
