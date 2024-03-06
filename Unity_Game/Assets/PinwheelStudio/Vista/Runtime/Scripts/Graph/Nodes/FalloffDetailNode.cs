#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Falloff Detail",
        path = "General/Falloff Detail",
        icon = "",
        documentation = "",
        keywords = "",
        description = "Adding minor detail to a base mask.")]
    public class FalloffDetailNode : ImageNodeBase
    {
        public readonly MaskSlot baseMaskSlot = new MaskSlot("Base", SlotDirection.Input, 0);
        public readonly MaskSlot detailMaskSlot = new MaskSlot("Detail", SlotDirection.Input, 1);

        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_detailMultiplier;
        public float detailMultiplier
        {
            get
            {
                return m_detailMultiplier;
            }
            set
            {
                m_detailMultiplier = Mathf.Clamp(value, 0f, 2f);
            }
        }

        private Material m_material;

        private readonly string SHADER_NAME = "Hidden/Vista/Graph/FalloffDetail";
        private readonly int BASE_MASK = Shader.PropertyToID("_BaseMask");
        private readonly int DETAIL_MASK = Shader.PropertyToID("_DetailMask");
        private readonly int DETAIL_MULTIPLIER = Shader.PropertyToID("_DetailMultiplier");
        private readonly int PASS = 0;

        public FalloffDetailNode() : base()
        {
            m_detailMultiplier = 1;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef baseRefLink = context.GetInputLink(m_id, baseMaskSlot.id);
            Texture baseMask = context.GetTexture(baseRefLink);
            int inputResolution;
            if (baseMask != null)
            {
                inputResolution = baseMask.width;
            }
            else
            {
                baseMask = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            SlotRef detailRefLink = context.GetInputLink(m_id, detailMaskSlot.id);
            Texture detailMask = context.GetTexture(detailRefLink);
            if (detailMask == null)
            {
                detailMask = Texture2D.blackTexture;
            }

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(BASE_MASK, baseMask);
            m_material.SetTexture(DETAIL_MASK, detailMask);
            m_material.SetFloat(DETAIL_MULTIPLIER, detailMultiplier);

            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(baseRefLink);
            context.ReleaseReference(detailRefLink);
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
