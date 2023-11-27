#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Value Check",
        path = "General/Value Check",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.om3pim1hr8og",
        keywords = "",
        description = "A simple way to check the content of an image. Pixels with value lower than 0 will be highlighted with green, higher than 1 will be highlighted with cyan.")]
    public class ValueCheckNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly ColorTextureSlot outputSlot = new ColorTextureSlot("Output", SlotDirection.Output, 100);

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/ValueCheck";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int PASS = 0;

        private Material m_material;

        public ValueCheckNode() : base()
        {

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
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.ARGB32);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
