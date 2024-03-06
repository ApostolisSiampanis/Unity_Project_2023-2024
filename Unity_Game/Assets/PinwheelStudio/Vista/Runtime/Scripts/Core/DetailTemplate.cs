#if VISTA
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Vista
{
    [CreateAssetMenu(menuName = "Vista/Detail Template")]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.61r5f6yn6s6k")]
    public class DetailTemplate : ScriptableObject
    {
        [SerializeField]
        protected DetailRenderMode m_renderMode;
        public DetailRenderMode renderMode
        {
            get
            {
                return m_renderMode;
            }
            set
            {
                m_renderMode = value;
            }
        }

        [SerializeField]
        protected Texture2D m_texture;
        public Texture2D texture
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
            }
        }

        [SerializeField]
        protected Texture2D[] m_textureVariants;
        public Texture2D[] textureVariants
        {
            get
            {
                if (!TemplateUtils.IsVariantsSupported())
                {
                    return new Texture2D[0];
                }

                if (m_textureVariants == null)
                {
                    return new Texture2D[0];
                }
                else
                {
                    Texture2D[] variants = new Texture2D[m_textureVariants.Length];
                    m_textureVariants.CopyTo(variants, 0);
                    return variants;
                }
            }
            set
            {
                if (value == null)
                {
                    m_textureVariants = new Texture2D[0];
                }
                else
                {
                    m_textureVariants = new Texture2D[value.Length];
                    value.CopyTo(m_textureVariants, 0);
                }
            }
        }

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
        protected Color m_primaryColor;
        public Color primaryColor
        {
            get
            {
                return m_primaryColor;
            }
            set
            {
                m_primaryColor = value;
            }
        }

        [SerializeField]
        protected Color m_secondaryColor;
        public Color secondaryColor
        {
            get
            {
                return m_secondaryColor;
            }
            set
            {
                m_secondaryColor = value;
            }
        }

        [SerializeField]
        protected float m_minHeight;
        public float minHeight
        {
            get
            {
                return m_minHeight;
            }
            set
            {
                m_minHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_maxHeight;
        public float maxHeight
        {
            get
            {
                return m_maxHeight;
            }
            set
            {
                m_maxHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_minWidth;
        public float minWidth
        {
            get
            {
                return m_minWidth;
            }
            set
            {
                m_minWidth = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_maxWidth;
        public float maxWidth
        {
            get
            {
                return m_maxWidth;
            }
            set
            {
                m_maxWidth = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_noiseSpread;
        public float noiseSpread
        {
            get
            {
                return m_noiseSpread;
            }
            set
            {
                m_noiseSpread = value;
            }
        }

        [SerializeField]
        protected float m_holeEdgePadding;
        public float holeEdgePadding
        {
            get
            {
                return m_holeEdgePadding;
            }
            set
            {
                m_holeEdgePadding = Mathf.Clamp01(value);
            }
        }

#if UNITY_2021_2_OR_NEWER
        [SerializeField]
        protected bool m_useInstancing;
        public bool useInstancing
        {
            get
            {
                return m_useInstancing;
            }
            set
            {
                m_useInstancing = value;
            }
        }
#endif

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
                m_pivotOffset = value;
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
        protected bool m_alignToSurface;
        public bool alignToSurface
        {
            get
            {
                return m_alignToSurface;
            }
            set
            {
                m_alignToSurface = value;
            }
        }

        [SerializeField]
        protected ShadowCastingMode m_castShadow;
        public ShadowCastingMode castShadow
        {
            get
            {
                return m_castShadow;
            }
            set
            {
                m_castShadow = value;
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
        protected int m_density;
        public int density
        {
            get
            {
                return m_density;
            }
            set
            {
                m_density = Mathf.Max(1, value);
            }
        }

        public void Reset()
        {
            m_renderMode = DetailRenderMode.Grass;
            m_texture = null;
            m_prefab = null;
            m_primaryColor = Color.white;
            m_secondaryColor = Color.white;
            m_minHeight = 0.5f;
            m_maxHeight = 1f;
            m_minWidth = 0.5f;
            m_maxWidth = 1f;
            m_noiseSpread = 0.1f;
            m_holeEdgePadding = 0;
#if UNITY_2021_2_OR_NEWER
            m_useInstancing = true;
#endif
            m_pivotOffset = 0;
            m_bendFactor = 1f;
            m_layer = 0;
            m_alignToSurface = false;
            m_castShadow = ShadowCastingMode.Off;
            m_receiveShadow = false;
            m_density = 100;
        }

        public bool IsValid()
        {
            if (m_renderMode == DetailRenderMode.Grass || m_renderMode == DetailRenderMode.GrassBillboard)
            {
                return m_texture != null;
            }
            else if (m_renderMode == DetailRenderMode.VertexLit)
            {
                return m_prefab != null;
            }
            return false;
        }
    }
}
#endif
