#if VISTA
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Vista
{
    [CreateAssetMenu(menuName = "Vista/Tree Template")]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.hi9q6ixwiw7i")]
    public class TreeTemplate : ScriptableObject
    {
        [SerializeField]
        protected GameObject m_prefab;
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
        protected int m_navMeshLod;
        public int navMeshLod
        {
            get
            {
                return m_navMeshLod;
            }
            set
            {
                m_navMeshLod = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_bendFactor;
        public float bendFactor
        {
            get
            {
                return m_bendFactor;
            }
            set
            {
                m_bendFactor = value;
            }
        }

        [SerializeField]
        protected BillboardAsset m_billboard;
        public BillboardAsset billboard
        {
            get
            {
                return m_billboard;
            }
            set
            {
                m_billboard = value;
            }
        }

        [SerializeField]
        protected ShadowCastingMode m_shadowCastingMode;
        public ShadowCastingMode shadowCastingMode
        {
            get
            {
                return m_shadowCastingMode;
            }
            set
            {
                m_shadowCastingMode = value;
            }
        }

        [SerializeField]
        protected bool m_receiveShadow;
        public bool receiveShadow
        {
            get
            {
                return m_receiveShadow;
            }
            set
            {
                m_receiveShadow = value;
            }
        }

        [SerializeField]
        protected ShadowCastingMode m_billboardShadowCastingMode;
        public ShadowCastingMode billboardShadowCastingMode
        {
            get
            {
                return m_billboardShadowCastingMode;
            }
            set
            {
                m_billboardShadowCastingMode = value;
            }
        }

        [SerializeField]
        protected bool m_billboardReceiveShadow;
        public bool billboardReceiveShadow
        {
            get
            {
                return m_billboardReceiveShadow;
            }
            set
            {
                m_billboardReceiveShadow = value;
            }
        }

        [SerializeField]
        protected int m_layer;
        public int layer
        {
            get
            {
                return m_layer;
            }
            set
            {
                m_layer = value;
            }
        }

        [SerializeField]
        protected bool m_keepPrefabLayer;
        public bool keepPrefabLayer
        {
            get
            {
                return m_keepPrefabLayer;
            }
            set
            {
                m_keepPrefabLayer = value;
            }
        }

        [SerializeField]
        protected float m_pivotOffset;
        public float pivotOffset
        {
            get
            {
                return m_pivotOffset;
            }
            set
            {
                m_pivotOffset = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        protected Quaternion m_baseRotation = Quaternion.identity;
        public Quaternion baseRotation
        {
            get
            {
                if (m_baseRotation.x == 0 &&
                    m_baseRotation.y == 0 &&
                    m_baseRotation.z == 0 &&
                    m_baseRotation.w == 0)
                {
                    m_baseRotation = Quaternion.identity;
                }
                return m_baseRotation;
            }
            set
            {
                m_baseRotation = value;
                if (m_baseRotation.x == 0 &&
                     m_baseRotation.y == 0 &&
                     m_baseRotation.z == 0 &&
                     m_baseRotation.w == 0)
                {
                    m_baseRotation = Quaternion.identity;
                }
            }
        }

        [SerializeField]
        protected Vector3 m_baseScale = Vector3.one;
        public Vector3 baseScale
        {
            get
            {
                return m_baseScale;
            }
            set
            {
                m_baseScale = value;
            }
        }

        public void Reset()
        {
            m_prefab = null;
            m_prefabVariants = null;
            m_navMeshLod = 0;
            m_bendFactor = 0;
            m_billboard = null;
            m_shadowCastingMode = ShadowCastingMode.On;
            m_receiveShadow = true;
            m_billboardShadowCastingMode = ShadowCastingMode.Off;
            m_billboardReceiveShadow = true;
            m_layer = 0;
            m_keepPrefabLayer = false;
            m_pivotOffset = 0;
            m_baseRotation = Quaternion.identity;
            m_baseScale = Vector3.one;
        }

        public bool IsValid()
        {
            return m_prefab != null;
        }
    }
}
#endif
