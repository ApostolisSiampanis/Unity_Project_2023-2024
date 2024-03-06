#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Rotation",
        path = "General/Rotation",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.4w7yppiqoknx",
        keywords = "",
        description = "Extract the rotation of input geometry.\nThe value is corresponding the angle of the normal vector on XY plane.")]
    public class RotationNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private bool m_remap01;
        public bool remap01
        {
            get
            {
                return m_remap01;
            }
            set
            {
                m_remap01 = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Rotation";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int PASS = 0;

        private static readonly string KW_REMAP_01 = "REMAP_01";

        private Material m_material;

        public RotationNode() : base()
        {
            m_remap01 = false;
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
            if (m_remap01)
            {
                m_material.EnableKeyword(KW_REMAP_01);
            }
            else
            {
                m_material.DisableKeyword(KW_REMAP_01);
            }

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
