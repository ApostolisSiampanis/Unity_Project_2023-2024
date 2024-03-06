#if VISTA

namespace Pinwheel.Vista.Graph
{
    public interface IEdge : IHasID
    {
        SlotRef outputSlot { get; set; }
        SlotRef inputSlot { get; set; }
    }
}
#endif