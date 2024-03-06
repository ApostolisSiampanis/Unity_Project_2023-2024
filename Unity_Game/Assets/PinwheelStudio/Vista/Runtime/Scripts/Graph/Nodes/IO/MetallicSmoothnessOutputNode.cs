#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Metallic Smoothness Output",
        path = "IO/Metallic Smoothness Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.d23jbl4mo2c",
        keywords = "",
        description = "Output the Metallic Smoothness Map to the terrain, embedded in R and A channel, correspondingly.\nMainly for Polaris and custom terrain since Unity terrain doesn't use this map.")]
    public class MetallicSmoothnessOutputNode : ImageNodeBase, IOutputNode
    {
        public readonly MaskSlot metallicInput = new MaskSlot("Metallic", SlotDirection.Input, 0);
        public readonly MaskSlot smoothnessInput = new MaskSlot("Smoothness", SlotDirection.Input, 1);
        public readonly ColorTextureSlot outputSlot = new ColorTextureSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_metallicMultiplier;
        public float metallicMultiplier
        {
            get
            {
                return m_metallicMultiplier;
            }
            set
            {
                m_metallicMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_smoothnessMultiplier;
        public float smoothnessMultiplier
        {
            get
            {
                return m_smoothnessMultiplier;
            }
            set
            {
                m_smoothnessMultiplier = Mathf.Max(0, value);
            }
        }

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

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/MetallicSmoothnessOutput";
        private static readonly int METALLIC = Shader.PropertyToID("_Metallic");
        private static readonly int METALLIC_MULTIPLIER = Shader.PropertyToID("_MetallicMultiplier");
        private static readonly int SMOOTHNESS = Shader.PropertyToID("_Smoothness");
        private static readonly int SMOOTHNESS_MULTIPLIER = Shader.PropertyToID("_SmoothnessMultiplier");
        private static readonly int PASS = 0;
        private Material m_material;

        public MetallicSmoothnessOutputNode() : base()
        {
            m_metallicMultiplier = 1f;
            m_smoothnessMultiplier = 1f;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef metallicRefLink = context.GetInputLink(m_id, metallicInput.id);
            Texture metallicTexture = context.GetTexture(metallicRefLink);
            if (metallicTexture == null)
            {
                metallicTexture = Texture2D.blackTexture;
            }

            SlotRef smoothnessRefLink = context.GetInputLink(m_id, smoothnessInput.id);
            Texture smoothnessTexture = context.GetTexture(smoothnessRefLink);
            if (smoothnessTexture == null)
            {
                smoothnessTexture = Texture2D.blackTexture;
            }

            int inputResolution;
            if (metallicTexture != null)
            {
                inputResolution = metallicTexture.width;
            }
            else if (smoothnessTexture != null)
            {
                inputResolution = smoothnessTexture.width;
            }
            else
            {
                inputResolution = baseResolution;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.ARGB32);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(METALLIC, metallicTexture);
            m_material.SetFloat(METALLIC_MULTIPLIER, m_metallicMultiplier);
            m_material.SetTexture(SMOOTHNESS, smoothnessTexture);
            m_material.SetFloat(SMOOTHNESS_MULTIPLIER, m_smoothnessMultiplier);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(metallicRefLink);
            context.ReleaseReference(smoothnessRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
