#if VISTA

namespace Pinwheel.Vista.Graph
{
    public interface IHasDynamicSlotCount
    {
        delegate void SlotsChangedHandler(INode sender);
        event SlotsChangedHandler slotsChanged;
    }
}
#endif
