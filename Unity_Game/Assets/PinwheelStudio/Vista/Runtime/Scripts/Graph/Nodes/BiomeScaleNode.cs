#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Biome Scale",
        path = "General/Biome Scale",
        icon = "",
        documentation = "",
        keywords = "",
        description = "Retrieve the biome scale value as mask map. This value can be multiplied with your height map to dynamically adjust the biome max height using its transform.")]
    public class BiomeScaleNode : ImageNodeBase
    {
        public readonly MaskSlot scaleYOutputSlot = new MaskSlot("Y", SlotDirection.Output, 101);

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/ConstantHeight"; //use the same shader with the Constant Height node
        private static readonly int VALUE = Shader.PropertyToID("_Value");
        private static readonly int PASS = 0;

        private Material m_material;

        public BiomeScaleNode() : base()
        {
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            Vector4 biomeScale = context.GetArg(Args.BIOME_SCALE).vectorValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, scaleYOutputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetFloat(VALUE, biomeScale.y);
            Drawing.DrawQuad(targetRt, m_material, PASS);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
