#if VISTA

namespace Pinwheel.Vista.Graph
{
    public interface ISlot
    {
        int id { get; }
        SlotDirection direction { get; }
        string name { get; }
        ISlotAdapter GetAdapter();
    }
}
#endif
