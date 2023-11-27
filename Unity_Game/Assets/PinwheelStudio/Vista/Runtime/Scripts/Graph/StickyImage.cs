#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class StickyImage : IStickyImage
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
        protected string m_groupId;
        public string groupId
        {
            get
            {
                return m_groupId;
            }
            set
            {
                m_groupId = value;
            }
        }

        [SerializeField]
        protected string m_textureGuid;
        public string textureGuid
        {
            get
            {
                return m_textureGuid;
            }
            set
            {
                m_textureGuid = value;
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

        public StickyImage()
        {
            m_id = System.Guid.NewGuid().ToString();
            m_groupId = string.Empty;
        }
    }
}
#endif
