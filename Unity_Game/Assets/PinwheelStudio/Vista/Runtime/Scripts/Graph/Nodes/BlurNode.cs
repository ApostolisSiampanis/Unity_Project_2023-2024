#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Blur",
        path = "Adjustments/Blur",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.tm5766iv32ze",
        keywords = "blur",
        description = "Perform Gaussian blur filter on the image.\nIt's expensive for high radius since it executes in one frame, consider using Smooth node if you need progressive operation.")]
    public class BlurNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private int m_radius;
        public int radius
        {
            get
            {
                return m_radius;
            }
            set
            {
                m_radius = Mathf.Clamp(value, 0, 100);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Blur";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int RADIUS = Shader.PropertyToID("_Radius");

        private Material m_material;

        public BlurNode() : base()
        {
            m_radius = 1;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            Texture inputTexture = context.GetTexture(inputRefLink);
            int inputResolution;
            if (inputTexture != null)
            {
                inputResolution = inputTexture.width;
            }
            else
            {
                inputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(RADIUS, m_radius);
            Drawing.DrawQuad(targetRt, m_material, m_radius);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }
    }
}
#endif
