#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class Group : IGroup
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
        protected string m_title;
        public string title
        {
            get
            {
                return m_title;
            }
            set
            {
                m_title = value;
            }
        }

        [SerializeField]
        protected Rect m_position;
        public Rect position
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

        public Group()
        {
            this.m_id = System.Guid.NewGuid().ToString();
        }
    }
}
#endif
