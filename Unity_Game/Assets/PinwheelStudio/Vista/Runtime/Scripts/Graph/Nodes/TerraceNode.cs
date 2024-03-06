#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Terrace",
        path = "Nature/Terrace",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ar5opzjvw9uz",
        keywords = "terrace, step",
        description = "Apply a terrace/quantize effect on the geometry.\nBest practice: Chain 2 or more of this node with different step height, use in conjunction with erosion nodes will yield impressive result.")]
    public class TerraceNode : ImageNodeBase
    {
        public readonly MaskSlot inputHeightSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot inputMaskSlot = new MaskSlot("Mask", SlotDirection.Input, 1);

        public readonly MaskSlot outputHeightSlot = new MaskSlot("Height", SlotDirection.Output, 100);
        public readonly MaskSlot outputMaskSlot = new MaskSlot("Mask", SlotDirection.Output, 101);
        public readonly MaskSlot outputOutlineSlot = new MaskSlot("Outline", SlotDirection.Output, 102);

        [SerializeField]
        private float m_stepHeight;
        public float stepHeight
        {
            get
            {
                return m_stepHeight;
            }
            set
            {
                m_stepHeight = Mathf.Max(0.1f, value);
            }
        }

        [SerializeField]
        private AnimationCurve m_remapCurve;
        public AnimationCurve remapCurve
        {
            get
            {
                return m_remapCurve;
            }
            set
            {
                m_remapCurve = value;
            }
        }

        [SerializeField]
        private bool m_useHeightMask;
        public bool useHeightMask
        {
            get
            {
                return m_useHeightMask;
            }
            set
            {
                m_useHeightMask = value;
            }
        }

        [SerializeField]
        private float m_minHeight;
        public float minHeight
        {
            get
            {
                return m_minHeight;
            }
            set
            {
                m_minHeight = Mathf.Max(0, Mathf.Min(m_maxHeight, value));
            }
        }

        [SerializeField]
        private float m_maxHeight;
        public float maxHeight
        {
            get
            {
                return m_maxHeight;
            }
            set
            {
                m_maxHeight = Mathf.Max(0, Mathf.Max(m_minHeight, value));
            }
        }

        [SerializeField]
        private AnimationCurve m_heightBlend;
        public AnimationCurve heightBlend
        {
            get
            {
                return m_heightBlend;
            }
            set
            {
                m_heightBlend = value;
            }
        }

        [SerializeField]
        private bool m_useSlopeMask;
        public bool useSlopeMask
        {
            get
            {
                return m_useSlopeMask;
            }
            set
            {
                m_useSlopeMask = value;
            }
        }

        [SerializeField]
        private float m_minSlope;
        public float minSlope
        {
            get
            {
                return m_minSlope;
            }
            set
            {
                m_minSlope = Mathf.Clamp(Mathf.Min(m_maxSlope, value), 0, 90);
            }
        }

        [SerializeField]
        private float m_maxSlope;
        public float maxSlope
        {
            get
            {
                return m_maxSlope;
            }
            set
            {
                m_maxSlope = Mathf.Clamp(Mathf.Max(m_minSlope, value), 0, 90);
            }
        }

        [SerializeField]
        private AnimationCurve m_slopeBlend;
        public AnimationCurve slopeBlend
        {
            get
            {
                return m_slopeBlend;
            }
            set
            {
                m_slopeBlend = value;
            }
        }

        [SerializeField]
        private OutlinePosition m_outlinePosition;
        public OutlinePosition outlinePosition
        {
            get
            {
                return m_outlinePosition;
            }
            set
            {
                m_outlinePosition = value;
            }
        }

        [SerializeField]
        private float m_outlineTolerance;
        public float outlineTolerance
        {
            get
            {
                return m_outlineTolerance;
            }
            set
            {
                m_outlineTolerance = Mathf.Clamp01(value);
            }
        }

        public enum OutlinePosition
        {
            Start, End
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Terrace";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int STEP_HEIGHT = Shader.PropertyToID("_StepHeight");
        private static readonly int REMAP_TEX = Shader.PropertyToID("_RemapTex");

        private static readonly string KW_HEIGHT_BLEND = "HEIGHT_BLEND";
        private static readonly int MIN_HEIGHT = Shader.PropertyToID("_MinHeight");
        private static readonly int MAX_HEIGHT = Shader.PropertyToID("_MaxHeight");
        private static readonly int HEIGHT_BLEND_TEXTURE = Shader.PropertyToID("_HeightBlendTexture");
        private static readonly int TERRAIN_SIZE = Shader.PropertyToID("_TerrainSize");

        private static readonly string KW_SLOPE_BLEND = "SLOPE_BLEND";
        private static readonly int MIN_SLOPE = Shader.PropertyToID("_MinSlope");
        private static readonly int MAX_SLOPE = Shader.PropertyToID("_MaxSlope");
        private static readonly int SLOPE_BLEND_TEXTURE = Shader.PropertyToID("_SlopeBlendTexture");

        private static readonly int OUTLINE_POSITION = Shader.PropertyToID("_OutlinePosition");
        private static readonly int OUTLINE_TOLERANCE = Shader.PropertyToID("_OutlineTolerance");

        private static readonly string KW_MASK_BLEND = "MASK_BLEND";
        private static readonly int MASK_TEXTURE = Shader.PropertyToID("_MaskTexture");

        private static readonly int PASS_HEIGHT = 0;
        private static readonly int PASS_MASK = 1;
        private static readonly int PASS_OUTLINE = 2;

        private Material m_material;

        public TerraceNode() : base()
        {
            m_stepHeight = 5;
            m_remapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            m_useHeightMask = false;
            m_minHeight = 0;
            m_maxHeight = 1000;
            m_heightBlend = AnimationCurve.Linear(0, 1, 1, 1);

            m_useSlopeMask = false;
            m_minSlope = 0;
            m_maxSlope = 90;
            m_slopeBlend = AnimationCurve.Linear(0, 1, 1, 1);

            m_outlinePosition = OutlinePosition.Start;
            m_outlineTolerance = 0.2f;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef inputHeightRefLink = context.GetInputLink(m_id, inputHeightSlot.id);
            Texture inputHeightTexture = context.GetTexture(inputHeightRefLink);
            int inputResolution;
            if (inputHeightTexture == null)
            {
                inputHeightTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputHeightTexture.width;
            }

            SlotRef inputMaskRefLink = context.GetInputLink(m_id, inputMaskSlot.id);
            Texture inputMaskTexture = context.GetTexture(inputMaskRefLink);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 terrainSize = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);

            m_material.SetTexture(MAIN_TEX, inputHeightTexture);
            m_material.SetFloat(STEP_HEIGHT, m_stepHeight / terrainHeight);
            m_material.SetVector(TERRAIN_SIZE, terrainSize);
            Texture2D remapTex = Utilities.TextureFromCurve(m_remapCurve);
            m_material.SetTexture(REMAP_TEX, remapTex);

            m_material.SetKeywordEnable(KW_HEIGHT_BLEND, m_useHeightMask);
            m_material.SetFloat(MIN_HEIGHT, m_minHeight);
            m_material.SetFloat(MAX_HEIGHT, m_maxHeight);
            Texture2D heightBlendTexture = Utilities.TextureFromCurve(m_heightBlend);
            m_material.SetTexture(HEIGHT_BLEND_TEXTURE, heightBlendTexture);

            m_material.SetKeywordEnable(KW_SLOPE_BLEND, m_useSlopeMask);
            m_material.SetFloat(MIN_SLOPE, m_minSlope * Mathf.Deg2Rad);
            m_material.SetFloat(MAX_SLOPE, m_maxSlope * Mathf.Deg2Rad);
            Texture2D slopeBlendTexture = Utilities.TextureFromCurve(m_slopeBlend);
            m_material.SetTexture(SLOPE_BLEND_TEXTURE, slopeBlendTexture);

            m_material.SetKeywordEnable(KW_MASK_BLEND, inputMaskTexture != null);
            if (inputMaskTexture != null)
            {
                m_material.SetTexture(MASK_TEXTURE, inputMaskTexture);
            }

            m_material.SetInt(OUTLINE_POSITION, (int)m_outlinePosition);
            m_material.SetFloat(OUTLINE_TOLERANCE, m_outlineTolerance);

            int resolution = this.CalculateResolution(baseResolution, inputResolution);

            SlotRef outputHeightRef = new SlotRef(m_id, outputHeightSlot.id);
            if (context.IsTargetNode(m_id) || context.GetReferenceCount(outputHeightRef) > 0)
            {
                DataPool.RtDescriptor outputHeightDesc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture outputHeightRt = context.CreateRenderTarget(outputHeightDesc, outputHeightRef);
                Drawing.DrawQuad(outputHeightRt, m_material, PASS_HEIGHT);
            }

            SlotRef outputMaskRef = new SlotRef(m_id, outputMaskSlot.id);
            if (context.GetReferenceCount(outputMaskRef) > 0)
            {
                DataPool.RtDescriptor outputMaskDesc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture outputMaskRt = context.CreateRenderTarget(outputMaskDesc, outputMaskRef);
                Drawing.DrawQuad(outputMaskRt, m_material, PASS_MASK);
            }

            SlotRef outputOutlineRef = new SlotRef(m_id, outputOutlineSlot.id);
            if (context.GetReferenceCount(outputOutlineRef) > 0)
            {
                DataPool.RtDescriptor outlineDesc = DataPool.RtDescriptor.Create(resolution, resolution);
                RenderTexture outlineRt = context.CreateRenderTarget(outlineDesc, outputOutlineRef);
                Drawing.DrawQuad(outlineRt, m_material, PASS_OUTLINE);
            }

            context.ReleaseReference(inputHeightRefLink);
            context.ReleaseReference(inputMaskRefLink);
            Object.DestroyImmediate(remapTex);
            Object.DestroyImmediate(heightBlendTexture);
            Object.DestroyImmediate(slopeBlendTexture);
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
