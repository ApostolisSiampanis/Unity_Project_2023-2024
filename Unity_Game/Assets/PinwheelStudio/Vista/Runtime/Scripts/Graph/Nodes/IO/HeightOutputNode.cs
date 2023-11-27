#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Height Output",
        path = "IO/Height Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ybc4kfbz310y",
        keywords = "",
        description = "Output the height map to the terrain.")]
    public class HeightOutputNode : ExecutableNodeBase, IOutputNode
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 10);

        public SlotRef mainOutputSlot
        {
            get
            {
                return new SlotRef(m_id, outputSlot.id);
            }
        }

        public override bool isBypassed
        {
            get
            {
                return false;
            }
            set
            {
                m_isBypassed = false;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/HeightOutput";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int PASS = 0;
        private Material m_material;

        public HeightOutputNode() : base()
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
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            Texture inputTexture = context.GetTexture(inputRefLink);
            if (inputTexture == null)
            {
                inputTexture = Texture2D.blackTexture;
            }

            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(baseResolution, baseResolution, RenderTextureFormat.RFloat);
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
