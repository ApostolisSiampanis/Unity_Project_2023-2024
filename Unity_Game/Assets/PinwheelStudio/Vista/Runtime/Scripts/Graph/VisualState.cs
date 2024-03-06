#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public struct VisualState
    {
        [SerializeField]
        private Vector2 m_position;
        public Vector2 position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }

        [SerializeField]
        private bool m_collapsed;
        public bool collapsed
        {
            get
            {
                return m_collapsed;
            }
            set
            {
                m_collapsed = value;
            }
        }
    }
}
#endif
