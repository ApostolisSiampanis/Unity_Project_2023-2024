#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public struct SlotRef : IEquatable<SlotRef>
    {
        public static readonly SlotRef invalid = new SlotRef(string.Empty, int.MinValue);

        [SerializeField]
        private string m_nodeId;
        public string nodeId
        {
            get
            {
                return m_nodeId;
            }
        }

        [SerializeField]
        private int m_slotId;
        public int slotId
        {
            get
            {
                return m_slotId;
            }
        }

        public SlotRef(string nodeId, int slotId)
        {
            this.m_nodeId = nodeId;
            this.m_slotId = slotId;
        }

        public bool Equals(SlotRef other)
        {
            return m_nodeId.Equals(other.m_nodeId) && m_slotId.Equals(other.m_slotId);
        }

        public override string ToString()
        {
            return $"({slotId},{nodeId})";
        }
    }
}
#endif