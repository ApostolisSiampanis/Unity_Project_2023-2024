#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Texture Output",
        path = "IO/Texture Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.u7jpy58elyd4",
        keywords = "",
        description = "Output the terrain's weight map (or splat control map, alpha map) of a terrain layer.\nTips: Use Weight Blend node to adjust the weight map to ensure the final result is not overshoot.")]
    public class TextureOutputNode : ImageNodeBase, IOutputNode
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        public SlotRef mainOutputSlot
        {
            get
            {
                return new SlotRef(m_id, outputSlot.id);
            }
        }

        [SerializeAsset]
        private TerrainLayer m_terrainLayer;
        public TerrainLayer terrainLayer
        {
            get
            {
                return m_terrainLayer;
            }
            set
            {
                m_terrainLayer = value;
            }
        }

        [SerializeField]
        private int m_order;
        public int order
        {
            get
            {
                return m_order;
            }
            set
            {
                m_order = value;
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

        public TextureOutputNode() : base()
        {
            m_order = 0;
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
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            Drawing.Blit01(inputTexture, targetRt);
            context.ReleaseReference(inputRefLink);
        }
    }
}
#endif
