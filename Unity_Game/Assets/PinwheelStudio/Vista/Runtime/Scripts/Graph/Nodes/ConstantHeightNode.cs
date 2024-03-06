#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Constant Height",
        path = "Base Shape/Constant Height",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.gf7p67gwoflf",
        keywords = "height, flat",
        description = "Remap the world space height to [0,1].")]
    public class ConstantHeightNode : ImageNodeBase
    {
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
                m_targetHeight = Mathf.Max(0, value);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/ConstantHeight";
        private static readonly int VALUE = Shader.PropertyToID("_Value");
        private static readonly int PASS = 0;

        private Material m_material;

        public ConstantHeightNode() : base()
        {
            m_targetHeight = 0;
            m_resolutionOverride = ResolutionOverrideOptions.Absolute;
            m_resolutionAbsolute = 8;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetFloat(VALUE, m_targetHeight * 1.0f / terrainHeight);
            Drawing.DrawQuad(targetRt, m_material, PASS);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
