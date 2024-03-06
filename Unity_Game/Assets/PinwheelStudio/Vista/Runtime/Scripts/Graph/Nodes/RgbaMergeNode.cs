#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "RGBA Merge",
        path = "General/RGBA Merge",
        icon = "",
        documentation = "",
        keywords = "pack, combine",
        description = "Merge 4 grayscale masks into a single RGBA texture.")]
    public class RgbaMergeNode : ImageNodeBase
    {
        public readonly MaskSlot rInputSlot = new MaskSlot("R", SlotDirection.Input, 0);
        public readonly MaskSlot gInputSlot = new MaskSlot("G", SlotDirection.Input, 1);
        public readonly MaskSlot bInputSlot = new MaskSlot("B", SlotDirection.Input, 2);
        public readonly MaskSlot aInputSlot = new MaskSlot("A", SlotDirection.Input, 3);

        public readonly ColorTextureSlot outputSlot = new ColorTextureSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_rMultiplier;
        public float rMultiplier
        {
            get
            {
                return m_rMultiplier;
            }
            set
            {
                m_rMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_gMultiplier;
        public float gMultiplier
        {
            get
            {
                return m_gMultiplier;
            }
            set
            {
                m_gMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_bMultiplier;
        public float bMultiplier
        {
            get
            {
                return m_bMultiplier;
            }
            set
            {
                m_bMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_aMultiplier;
        public float aMultiplier
        {
            get
            {
                return m_aMultiplier;
            }
            set
            {
                m_aMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private string m_titleLabel;
        [NonExposable]
        public string titleLabel
        {
            get
            {
                return m_titleLabel;
            }
            set
            {
                m_titleLabel = value;
            }
        }

        [SerializeField]
        private string m_rLabel;
        [NonExposable]
        public string rLabel
        {
            get
            {
                if (string.IsNullOrEmpty(m_rLabel))
                {
                    return "R";
                }
                else
                {
                    return m_rLabel;
                }
            }
            set
            {
                m_rLabel = value;
            }
        }

        [SerializeField]
        private string m_gLabel;
        [NonExposable]
        public string gLabel
        {
            get
            {
                if (string.IsNullOrEmpty(m_gLabel))
                {
                    return "G";
                }
                else
                {
                    return m_gLabel;
                }
            }
            set
            {
                m_gLabel = value;
            }
        }

        [SerializeField]
        private string m_bLabel;
        [NonExposable]
        public string bLabel
        {
            get
            {
                if (string.IsNullOrEmpty(m_bLabel))
                {
                    return "B";
                }
                else
                {
                    return m_bLabel;
                }
            }
            set
            {
                m_bLabel = value;
            }
        }

        [SerializeField]
        private string m_aLabel;
        [NonExposable]
        public string aLabel
        {
            get
            {
                if (string.IsNullOrEmpty(m_aLabel))
                {
                    return "A";
                }
                else
                {
                    return m_aLabel;
                }
            }
            set
            {
                m_aLabel = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/RgbaMerge";
        private static readonly int R_TEX = Shader.PropertyToID("_RTex");
        private static readonly int G_TEX = Shader.PropertyToID("_GTex");
        private static readonly int B_TEX = Shader.PropertyToID("_BTex");
        private static readonly int A_TEX = Shader.PropertyToID("_ATex");
        private static readonly int MULTIPLIER = Shader.PropertyToID("_Multiplier");
        private static readonly int PASS = 0;

        private Material m_material;

        public RgbaMergeNode() : base()
        {
            m_rMultiplier = 1;
            m_gMultiplier = 1;
            m_bMultiplier = 1;
            m_aMultiplier = 1;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int inputResolution;
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;

            SlotRef aInputRefLink = context.GetInputLink(m_id, aInputSlot.id);
            Texture aInputTexture = context.GetTexture(aInputRefLink);
            if (aInputTexture == null)
            {
                aInputTexture = Texture2D.whiteTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = aInputTexture.width;
            }

            SlotRef bInputRefLink = context.GetInputLink(m_id, bInputSlot.id);
            Texture bInputTexture = context.GetTexture(bInputRefLink);
            if (bInputTexture == null)
            {
                bInputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = bInputTexture.width;
            }

            SlotRef gInputRefLink = context.GetInputLink(m_id, gInputSlot.id);
            Texture gInputTexture = context.GetTexture(gInputRefLink);
            if (gInputTexture == null)
            {
                gInputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = gInputTexture.width;
            }

            SlotRef rInputRefLink = context.GetInputLink(m_id, rInputSlot.id);
            Texture rInputTexture = context.GetTexture(rInputRefLink);
            if (rInputTexture == null)
            {
                rInputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = rInputTexture.width;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.ARGB32);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(R_TEX, rInputTexture);
            m_material.SetTexture(G_TEX, gInputTexture);
            m_material.SetTexture(B_TEX, bInputTexture);
            m_material.SetTexture(A_TEX, aInputTexture);
            m_material.SetVector(MULTIPLIER, new Vector4(m_rMultiplier, m_gMultiplier, m_bMultiplier, m_aMultiplier));

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(aInputRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
