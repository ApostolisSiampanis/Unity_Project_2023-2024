#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Directional Warp",
        path = "Adjustments/Directional Warp",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ocuprwc49y9i",
        keywords = "",
        description = "Adding distortion to the image where the direction mask will be converted to a normal map to define the warp direction.")]
    public class DirectionalWarpNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot directionSlot = new MaskSlot("Direction", SlotDirection.Input, 1);
        public readonly MaskSlot intensitySlot = new MaskSlot("Intensity", SlotDirection.Input, 2);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_intensityMultiplier;
        public float intensityMultiplier
        {
            get
            {
                return m_intensityMultiplier;
            }
            set
            {
                m_intensityMultiplier = Mathf.Clamp(value, -1, 1);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/DirectionalWarp";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int DIRECTION_MAP = Shader.PropertyToID("_DirectionMap");
        private static readonly int INTENSITY_MAP = Shader.PropertyToID("_IntensityMap");
        private static readonly int INTENSITY_MULTIPLIER = Shader.PropertyToID("_IntensityMultiplier");
        private static readonly int PASS = 0;

        private Material m_material;

        public DirectionalWarpNode() : base()
        {
            m_intensityMultiplier = 1;
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

            SlotRef directionRefLink = context.GetInputLink(m_id, directionSlot.id);
            Texture directionTexture = context.GetTexture(directionRefLink);
            if (directionTexture == null)
            {
                directionTexture = Texture2D.whiteTexture;
            }

            SlotRef intensityRefLink = context.GetInputLink(m_id, intensitySlot.id);
            Texture intensityTexture = context.GetTexture(intensityRefLink);
            if (intensityTexture == null)
            {
                intensityTexture = Texture2D.whiteTexture;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetTexture(DIRECTION_MAP, directionTexture);
            m_material.SetTexture(INTENSITY_MAP, intensityTexture);
            m_material.SetFloat(INTENSITY_MULTIPLIER, m_intensityMultiplier);

            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(inputRefLink);
            context.ReleaseReference(directionRefLink);
            context.ReleaseReference(intensityRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
