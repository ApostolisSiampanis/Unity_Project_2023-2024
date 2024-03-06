#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    [CreateAssetMenu(menuName = "Vista/Object Template")]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.gk1w93mws9z")]
    public class ObjectTemplate : ScriptableObject
    {
        [SerializeField]
        private GameObject m_prefab;
        public GameObject prefab
        {
            get
            {
                return m_prefab;
            }
            set
            {
                m_prefab = value;
            }
        }

        [SerializeField]
        protected GameObject[] m_prefabVariants;
        public GameObject[] prefabVariants
        {
            get
            {
                if (!TemplateUtils.IsVariantsSupported())
                {
                    return new GameObject[0];
                }

                if (m_prefabVariants == null)
                {
                    return new GameObject[0];
                }
                else
                {
                    GameObject[] variants = new GameObject[m_prefabVariants.Length];
                    m_prefabVariants.CopyTo(variants, 0);
                    return variants;
                }
            }
            set
            {
                if (value == null)
                {
                    m_prefabVariants = new GameObject[0];
                }
                else
                {
                    m_prefabVariants = new GameObject[value.Length];
                    value.CopyTo(m_prefabVariants, 0);
                }
            }
        }

        [SerializeField]
        private bool m_alignToNormal;
        public bool alignToNormal
        {
            get
            {
                return m_alignToNormal;
            }
            set
            {
                m_alignToNormal = value;
            }
        }

        [SerializeField]
        private float m_normalAlignmentError;
        public float normalAlignmentError
        {
            get
            {
                return m_normalAlignmentError;
            }
            set
            {
                m_normalAlignmentError = Mathf.Clamp01(value);
            }
        }

        public void Reset()
        {
            m_prefab = null;
            m_alignToNormal = false;
            m_normalAlignmentError = 0;
        }

        public bool IsValid()
        {
            return m_prefab != null;
        }
    }
}
#endif
