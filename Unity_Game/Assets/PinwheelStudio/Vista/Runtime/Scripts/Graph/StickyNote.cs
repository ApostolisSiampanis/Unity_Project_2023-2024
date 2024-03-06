#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class StickyNote : IStickyNote
    {
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
        protected string m_contents;
        public string contents
        {
            get
            {
                return m_contents;
            }
            set
            {
                m_contents = value;
            }
        }

        [SerializeField]
        protected int m_fontSize;
        public int fontSize
        {
            get
            {
                return m_fontSize;
            }
            set
            {
                m_fontSize = value;
            }
        }

        [SerializeField]
        protected int m_theme;
        public int theme
        {
            get
            {
                return m_theme;
            }
            set
            {
                m_theme = value;
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

        public StickyNote()
        {
            m_id = System.Guid.NewGuid().ToString();
            m_groupId = string.Empty;
        }
    }
}
#endif
