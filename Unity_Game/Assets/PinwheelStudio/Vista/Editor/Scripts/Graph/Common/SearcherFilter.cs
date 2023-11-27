#if VISTA
using Pinwheel.Vista.Graph;
using System;

namespace Pinwheel.VistaEditor.Graph
{
    public struct SearcherFilter
    {
        public bool inspectSlot { get; set; }
        public SlotDirection slotDirection { get; set; }
        public ISlotAdapter slotAdapter { get; set; }
        public Predicate<Type> typeFilter { get; set; }
        public string sourceGraphPath { get; set; }
    }
}
#endif
