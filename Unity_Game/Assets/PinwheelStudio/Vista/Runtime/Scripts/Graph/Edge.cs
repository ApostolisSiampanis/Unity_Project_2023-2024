#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class Edge : IEdge
    {
        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        protected SlotRef m_outputSlot;
        public SlotRef outputSlot
        {
            get
            {
                return m_outputSlot;
            }
            set
            {
                m_outputSlot = value;
            }
        }

        [SerializeField]
        protected SlotRef m_inputSlot;
        public SlotRef inputSlot
        {
            get
            {
                return m_inputSlot;
            }
            set
            {
                m_inputSlot = value;
            }
        }

        public Edge()
        {
            this.m_id = Guid.NewGuid().ToString();
        }

        public Edge(SlotRef outputSlot, SlotRef inputSlot)
        {
            this.m_id = Guid.NewGuid().ToString();
            this.m_outputSlot = outputSlot;
            this.m_inputSlot = inputSlot;
        }
    }
}
#endif
