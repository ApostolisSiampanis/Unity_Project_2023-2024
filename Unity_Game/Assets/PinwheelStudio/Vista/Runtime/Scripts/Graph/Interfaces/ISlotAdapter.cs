#if VISTA
using System;

namespace Pinwheel.Vista.Graph
{
    public interface ISlotAdapter
    {
        Type slotType { get; }
        SlotDirection direction { get; }
        bool CanConnectTo(ISlotAdapter other);
    }
}
#endif
