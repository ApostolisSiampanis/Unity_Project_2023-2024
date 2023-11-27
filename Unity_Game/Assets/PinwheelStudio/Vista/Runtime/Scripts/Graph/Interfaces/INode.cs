#if VISTA

namespace Pinwheel.Vista.Graph
{
    public interface INode : IHasID, IGroupable
    {
        VisualState visualState { get; set; }
        ISlot[] GetInputSlots();
        ISlot[] GetOutputSlots();
        ISlot GetSlot(int id);
        INode ShallowCopy();
    }
}
#endif
