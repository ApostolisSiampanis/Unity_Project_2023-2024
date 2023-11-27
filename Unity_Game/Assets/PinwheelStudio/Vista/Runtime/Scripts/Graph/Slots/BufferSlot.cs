#if VISTA
using System;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class BufferSlot : SlotBase
    {
        public struct Adapter : ISlotAdapter
        {
            public Type slotType
            {
                get
                {
                    return typeof(BufferSlot);
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
                if (other.slotType.Equals(this.slotType))
                {
                    return true;
                }
                else if (other.slotType.Equals(typeof(GenericSlot)))
                {
                    return true;
                }
                return false;
            }

            public Adapter(SlotDirection dir)
            {
                m_direction = dir;
            }
        }

        public override ISlotAdapter GetAdapter()
        {
            return new Adapter(direction);
        }

        public BufferSlot() : base()
        { 
        }

        public BufferSlot(string name, SlotDirection direction, int id) : base(name, direction, id)
        {

        }
    }
}
#endif
