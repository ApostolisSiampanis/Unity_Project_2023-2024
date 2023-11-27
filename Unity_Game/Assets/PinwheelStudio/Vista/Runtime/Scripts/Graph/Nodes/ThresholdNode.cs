#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Threshold",
        path = "Adjustments/Threshold",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.g0vrpl4vb0no",
        keywords = "threshold, low, high",
        description = "Apply a threshold filter on the image. Value lower than Min will turn black, while value higher than Max will turn white.")]
    public class ThresholdNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        public enum CompareMode
        {
            Exclusive, Inclusive
        }

        [SerializeField]
        private float m_low;
        public float low
        {
            get
            {
                return m_low;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Min(v, m_high));
                m_low = v;
            }
        }

        [SerializeField]
        private float m_high;
        public float high
        {
            get
            {
                return m_high;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Max(v, m_low));
                m_high = v;
            }
        }

        [SerializeField]
        private CompareMode m_compareMode;
        public CompareMode compareMode
        {
            get
            {
                return m_compareMode;
            }
            set
            {
                m_compareMode = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Threshold";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int LOW = Shader.PropertyToID("_Low");
        private static readonly int HIGH = Shader.PropertyToID("_High");
        private static readonly int MODE = Shader.PropertyToID("_Mode");
        private static readonly int PASS = 0;

        private Material m_material;

        public ThresholdNode() : base()
        {
            m_low = 0f;
            m_high = 1f;
            m_compareMode = CompareMode.Exclusive;
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
            m_material.SetFloat(LOW, m_low);
            m_material.SetFloat(HIGH, m_high);
            m_material.SetFloat(MODE, (int)m_compareMode);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }
    }
}
#endif
