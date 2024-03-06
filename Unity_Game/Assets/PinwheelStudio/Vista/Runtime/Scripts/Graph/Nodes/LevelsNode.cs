#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Levels",
        path = "Adjustments/Levels",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.g0t60wz4n6yr",
        keywords = "level, bright, dark",
        description = "Apply a simple levels filter to the image.")]
    public class LevelsNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_inLow;
        public float inLow
        {
            get
            {
                return m_inLow;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Min(v, m_inHigh));
                m_inLow = v;
            }
        }

        [SerializeField]
        private float m_inMid;
        public float inMid
        {
            get
            {
                return m_inMid;
            }
            set
            {
                m_inMid = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float m_inHigh;
        public float inHigh
        {
            get
            {
                return m_inHigh;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Max(v, m_inLow));
                m_inHigh = v;
            }
        }

        [SerializeField]
        private float m_outLow;
        public float outLow
        {
            get
            {
                return m_outLow;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Min(v, m_outHigh));
                m_outLow = v;
            }
        }

        [SerializeField]
        private float m_outHigh;
        public float outHigh
        {
            get
            {
                return m_outHigh;
            }
            set
            {
                float v = value;
                v = Mathf.Clamp01(Mathf.Max(v, m_outLow));
                m_outHigh = v;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Levels";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int IN_LOW = Shader.PropertyToID("_InLow");
        private static readonly int IN_MID = Shader.PropertyToID("_InMid");
        private static readonly int IN_HIGH = Shader.PropertyToID("_InHigh");
        private static readonly int OUT_LOW = Shader.PropertyToID("_OutLow");
        private static readonly int OUT_HIGH = Shader.PropertyToID("_OutHigh");
        private static readonly int PASS = 0;

        private Material m_material;

        public LevelsNode() : base()
        {
            m_inLow = 0f;
            m_inMid = 0.5f;
            m_inHigh = 1f;

            m_outLow = 0f;
            m_outHigh = 1f;
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
            m_material.SetFloat(IN_LOW, m_inLow);
            m_material.SetFloat(IN_MID, Mathf.Lerp(m_inLow, m_inHigh, m_inMid));
            m_material.SetFloat(IN_HIGH, m_inHigh);
            m_material.SetFloat(OUT_LOW, m_outLow);
            m_material.SetFloat(OUT_HIGH, m_outHigh);

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
