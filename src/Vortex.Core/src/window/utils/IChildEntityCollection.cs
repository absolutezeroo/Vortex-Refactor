// @see core/window/utils/class_3412.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3412.as
public interface IChildEntityCollection
{
    int NumChildren { get; }

    IChildEntity GetChildAt(int index);

    IChildEntity? GetChildByID(uint id);

    IChildEntity? GetChildByName(string name);

    int GetChildIndex(IChildEntity child);

    uint GroupChildrenWithID(uint id, IList<IChildEntity> results);
}
