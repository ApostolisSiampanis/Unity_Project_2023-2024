#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Convex",
        path = "Masking/Convex",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.lhc6f26gyh9a",
        keywords = "",
        description = "Highlight the pixels that are higher than their neighbors.")]
    public class ConvexNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_epsilon;
        public float epsilon
        {
            get
            {
                return m_epsilon;
            }
            set
            {
                m_epsilon = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        private int m_tolerance;
        public int tolerance
        {
            get
            {
                return m_tolerance;
            }
            set
            {
                m_tolerance = Mathf.Clamp(value, 0, 7);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Convex";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int EPSILON = Shader.PropertyToID("_Epsilon");
        private static readonly int TOLERANCE = Shader.PropertyToID("_Tolerance");
        private static readonly int PASS = 0;

        private Material m_material;

        public ConvexNode() : base()
        {
            m_epsilon = 0;
            m_tolerance = 3;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
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
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(EPSILON, m_epsilon);
            m_material.SetFloat(TOLERANCE, m_tolerance);

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
