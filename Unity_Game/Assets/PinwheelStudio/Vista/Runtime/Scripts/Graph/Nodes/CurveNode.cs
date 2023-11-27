#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Curve",
        path = "Adjustments/Curve",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.f6ekjq2nyy2c",
        keywords = "curve",
        description = "Remap the image with a curve.")]
    public class CurveNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private AnimationCurve m_curve;
        public AnimationCurve curve
        {
            get
            {
                return m_curve;
            }
            set
            {
                m_curve = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Curve";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int CURVE_TEX = Shader.PropertyToID("_CurveTex");
        private static readonly int PASS = 0;

        private Material m_material;

        public CurveNode() : base()
        {
            m_curve = AnimationCurve.Linear(0, 0, 1, 1);
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

            Texture2D curveTexture = Utilities.TextureFromCurve(m_curve);
            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetTexture(CURVE_TEX, curveTexture);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(curveTexture);
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
