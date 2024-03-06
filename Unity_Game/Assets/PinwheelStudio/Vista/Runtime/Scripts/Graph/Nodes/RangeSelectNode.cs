#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Range Select",
        path = "Masking/Range Select",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.f86xohhcvz7m",
        keywords = "",
        description = "Select the pixels within a value range.")]
    public class RangeSelectNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_min;
        public float min
        {
            get
            {
                return m_min;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Min(v, m_max));
                m_min = v;
            }
        }

        [SerializeField]
        private float m_max;
        public float max
        {
            get
            {
                return m_max;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Max(v, m_min));
                m_max = v;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/RangeSelect";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MIN = Shader.PropertyToID("_Min");
        private static readonly int MAX = Shader.PropertyToID("_Max");
        private static readonly int PASS = 0;

        private Material m_material;

        public RangeSelectNode() : base()
        {
            m_min = 0f;
            m_max = 1f;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            Texture inputTexture = context.GetTexture(inputRefLink);
            int inputResolution;
            if (inputTexture == null)
            {
                inputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputTexture.width;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(MIN, m_min);
            m_material.SetFloat(MAX, m_max);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override void Bypass(GraphContext context)
        {
            return;
        }
    }
}
#endif
