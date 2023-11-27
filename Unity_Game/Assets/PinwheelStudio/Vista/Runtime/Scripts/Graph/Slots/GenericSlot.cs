#if VISTA
using System;
using System.ComponentModel;

namespace Pinwheel.Vista.Graph
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GenericSlot : SlotBase
    {
        public struct Adapter : ISlotAdapter
        {
            public Type slotType
            {
                get
                {
                    return typeof(GenericSlot);
                }
            }

            private SlotDirection m_direction;
            public SlotDirection direction
            {
                get
                {
                    return m_direction;
                }
            }

            public bool CanConnectTo(ISlotAdapter other)
            {
                if (other.direction.Equals(this.direction))
                    return false;
                return true;
            }

            public Adapter(SlotDirection direction)
            {
                m_direction = direction;
            }
        }

        public GenericSlot() : base()
        {

        }

        public GenericSlot(string name, SlotDirection type, int id) : base(name, type, id)
        {
        }

        public override ISlotAdapter GetAdapter()
        {
            return new Adapter(this.direction);
        }
    }
}
#endif
