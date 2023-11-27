#if VISTA
using System;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class MaskSlot : SlotBase
    {
        public struct Adapter : ISlotAdapter
        {
            public Type slotType
            {
                get
                {
                    return typeof(MaskSlot);
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
                else if (other.slotType.Equals(typeof(ColorTextureSlot)) && other.direction == SlotDirection.Input)
                {
                    return true;
                }
                else if (other.slotType.Equals(typeof(GenericSlot)))
                {
                    return true;
                }

                return false;
            }

            public Adapter(SlotDirection direction)
            {
                m_direction = direction;
            }
        }

        public MaskSlot() : base()
        {

        }

        public MaskSlot(string name, SlotDirection type, int id) : base(name, type, id)
        {
        }

        public override ISlotAdapter GetAdapter()
        {
            return new Adapter(direction);
        }
    }
}
#endif
