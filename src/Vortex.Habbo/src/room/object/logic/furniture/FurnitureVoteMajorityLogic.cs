namespace Vortex.Habbo.Room.Object.Logic;

using Messages;
using Data;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureVoteMajorityLogic (class_3465)
public class FurnitureVoteMajorityLogic : FurnitureMultiStateLogic
{
    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage dataMsg || Object == null)
        {
            return;
        }

        if (dataMsg.Data is VoteResultStuffData voteData)
        {
            Object.ModelController.SetNumber("furniture_vote_majority_result", voteData.Result);
        }
    }
}
