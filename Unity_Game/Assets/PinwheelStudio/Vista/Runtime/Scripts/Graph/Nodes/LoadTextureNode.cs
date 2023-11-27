#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Load Texture",
        path = "Data/Load Texture",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.78hsax6pg976",
        keywords = "texture, image, file, asset",
        description = "Load pixel data from a texture asset. You can load both color texture (RGBA, splat map, color map) or RAW file (height map). Raw images (.raw, .r16) will be imported as textures in the asset folder so you can pick them with the selector.")]
    public class LoadTextureNode : ImageNodeBase
    {
        public readonly ColorTextureSlot rgbaOutputSlot = new ColorTextureSlot("RGBA", SlotDirection.Output, 110);
        public readonly MaskSlot rOutputSlot = new MaskSlot("R", SlotDirection.Output, 100);
        public readonly MaskSlot gOutputSlot = new MaskSlot("G", SlotDirection.Output, 101);
        public readonly MaskSlot bOutputSlot = new MaskSlot("B", SlotDirection.Output, 102);
        public readonly MaskSlot aOutputSlot = new MaskSlot("A", SlotDirection.Output, 103);

        [SerializeAsset]
        private Texture m_texture;
        public Texture texture
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/LoadTexture";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int CHANNEL_INDEX = Shader.PropertyToID("_ChannelIndex");
        private static readonly int PASS_ALL_CHANNEL = 0;
        private static readonly int PASS_SINGLE_CHANNEL = 1;

        private Material m_material;

        public LoadTextureNode() : base()
        {
            m_texture = null;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            Texture inputTexture = m_texture != null ? m_texture : Texture2D.blackTexture;
            m_material.SetTexture(MAIN_TEX, inputTexture);

            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            SlotRef rgbaOutputRef = new SlotRef(m_id, rgbaOutputSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(rgbaOutputRef) > 0)
            {
                DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.ARGB32);
                RenderTexture targetRt = context.CreateRenderTarget(desc, rgbaOutputRef);
                Drawing.DrawQuad(targetRt, m_material, PASS_ALL_CHANNEL);
            }

            SlotRef rOutputRef = new SlotRef(m_id, rOutputSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(rOutputRef) > 0)
            {
                DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture targetRt = context.CreateRenderTarget(desc, rOutputRef);

                m_material.SetInt(CHANNEL_INDEX, 0);
                Drawing.DrawQuad(targetRt, m_material, PASS_SINGLE_CHANNEL);
            }

            SlotRef gOutputRef = new SlotRef(m_id, gOutputSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(gOutputRef) > 0)
            {
                DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture targetRt = context.CreateRenderTarget(desc, gOutputRef);

                m_material.SetInt(CHANNEL_INDEX, 1);
                Drawing.DrawQuad(targetRt, m_material, PASS_SINGLE_CHANNEL);
            }

            SlotRef bOutputRef = new SlotRef(m_id, bOutputSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(bOutputRef) > 0)
            {
                DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture targetRt = context.CreateRenderTarget(desc, bOutputRef);

                m_material.SetInt(CHANNEL_INDEX, 2);
                Drawing.DrawQuad(targetRt, m_material, PASS_SINGLE_CHANNEL);
            }

            SlotRef aOutputRef = new SlotRef(m_id, aOutputSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(aOutputRef) > 0)
            {
                DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture targetRt = context.CreateRenderTarget(desc, aOutputRef);

                m_material.SetInt(CHANNEL_INDEX, 3);
                Drawing.DrawQuad(targetRt, m_material, PASS_SINGLE_CHANNEL);
            }
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
