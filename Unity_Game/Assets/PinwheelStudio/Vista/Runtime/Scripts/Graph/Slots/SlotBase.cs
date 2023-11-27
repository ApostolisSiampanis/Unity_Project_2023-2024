#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public abstract class SlotBase : ISlot
    {
        [SerializeField]
        protected int m_id;
        public int id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        protected SlotDirection m_type;
        public SlotDirection direction
        {
            get
            {
                return m_type;
            }
        }

        [SerializeField]
        protected string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
        }

        public SlotBase()
        {

        }

        public SlotBase(string name, SlotDirection type, int id)
        {
            this.m_name = name;
            this.m_type = type;
            this.m_id = id;
        }

        public abstract ISlotAdapter GetAdapter();
    }
}
#endif
