#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Flatten",
        path = "General/Flatten",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.9ql4c0th4qgw",
        keywords = "",
        description = "Flatten the geometry using a mask.")]
    public class FlattenNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot maskSlot = new MaskSlot("Mask", SlotDirection.Input, 1);

        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_targetHeight;
        public float targetHeight
        {
            get
            {
                return m_targetHeight;
            }
            set
            {
                m_targetHeight = value;
            }
        }

        [SerializeField]
        private bool m_useWorldHeight;
        public bool useWorldHeight
        {
            get
            {
                return m_useWorldHeight;
            }
            set
            {
                m_useWorldHeight = value;
            }
        }

        private static readonly string SHADER = "Hidden/Vista/Graph/Flatten";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MASK = Shader.PropertyToID("_Mask");
        private static readonly int TARGET_HEIGHT = Shader.PropertyToID("_TargetHeight");
        private static readonly int PASS = 0;

        private Material m_material;

        public FlattenNode() : base()
        {
            m_targetHeight = 0.5f;
            m_useWorldHeight = false;
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

            SlotRef maskRefLink = context.GetInputLink(m_id, maskSlot.id);
            Texture maskTexture = context.GetTexture(maskRefLink);
            if (maskTexture == null)
            {
                maskTexture = Texture2D.blackTexture;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetTexture(MASK, maskTexture);

            float height = m_targetHeight;
            if (m_useWorldHeight)
            {
                float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
                height = m_targetHeight / terrainHeight;
            }
            m_material.SetFloat(TARGET_HEIGHT, height);

            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(inputRefLink);
            context.ReleaseReference(maskRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
