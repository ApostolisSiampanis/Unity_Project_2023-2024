#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Voronoi",
        path = "Base Shape/Voronoi",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.5fi034yw6nfi",
        keywords = "voronoi, worley, pattern",
        description = "Generate a fractal voronoi/worley pattern.")]
    public class VoronoiNode : ImageNodeBase, IHasSeed
    {
        public readonly MaskSlot cellsSlot = new MaskSlot("Cells", SlotDirection.Output, 100);
        public readonly MaskSlot rawCellsSlot = new MaskSlot("Raw Cells", SlotDirection.Output, 101);
        public readonly MaskSlot outlineSlot = new MaskSlot("Outline", SlotDirection.Output, 102);

        public enum WarpMode
        {
            None, Angular, Directional
        }

        [SerializeField]
        private Vector2 m_offset;
        public Vector2 offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        [SerializeField]
        private float m_scale;
        public float scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
            }
        }

        [SerializeField]
        private float m_lacunarity;
        public float lacunarity
        {
            get
            {
                return m_lacunarity;
            }
            set
            {
                m_lacunarity = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private float m_persistence;
        public float persistence
        {
            get
            {
                return m_persistence;
            }
            set
            {
                m_persistence = Mathf.Clamp(value, 0.01f, 1f);
            }
        }

        [SerializeField]
        private float m_amplitudeExponent;
        public float amplitudeExponent
        {
            get
            {
                return m_amplitudeExponent;
            }
            set
            {
                m_amplitudeExponent = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private int m_layerCount;
        public int layerCount
        {
            get
            {
                return m_layerCount;
            }
            set
            {
                m_layerCount = Mathf.Clamp(value, 1, 4);
            }
        }

        [SerializeField]
        private bool m_inverse;
        public bool inverse
        {
            get
            {
                return m_inverse;
            }
            set
            {
                m_inverse = value;
            }
        }

        [SerializeField]
        private int m_seed;
        public int seed
        {
            get
            {
                return m_seed;
            }
            set
            {
                m_seed = value;
            }
        }

        [SerializeField]
        private WarpMode m_warpMode;
        public WarpMode warpMode
        {
            get
            {
                return m_warpMode;
            }
            set
            {
                m_warpMode = value;
            }
        }

        [SerializeField]
        private float m_warpAngleMin;
        public float warpAngleMin
        {
            get
            {
                return m_warpAngleMin;
            }
            set
            {
                m_warpAngleMin = Mathf.Clamp(Mathf.Min(value, m_warpAngleMax), -360, 360);
            }
        }

        [SerializeField]
        private float m_warpAngleMax;
        public float warpAngleMax
        {
            get
            {
                return m_warpAngleMax;
            }
            set
            {
                m_warpAngleMax = Mathf.Clamp(Mathf.Max(value, m_warpAngleMin), -360, 360);
            }
        }

        [SerializeField]
        private float m_warpIntensity;
        public float warpIntensity
        {
            get
            {
                return m_warpIntensity;
            }
            set
            {
                m_warpIntensity = value;
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

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Voronoi";
        private static readonly int WORLD_BOUNDS = Shader.PropertyToID("_WorldBounds");
        private static readonly int OFFSET = Shader.PropertyToID("_Offset");
        private static readonly int SCALE = Shader.PropertyToID("_Scale");
        private static readonly int AMPLITUDE_EXP = Shader.PropertyToID("_AmplitudeExponent");
        private static readonly int LACUNARITY = Shader.PropertyToID("_Lacunarity");
        private static readonly int PERSISTENCE = Shader.PropertyToID("_Persistence");
        private static readonly int LAYER_COUNT = Shader.PropertyToID("_LayerCount");
        private static readonly int RANDOM_OFFSET = Shader.PropertyToID("_RandomOffset");
        private static readonly int INVERSE = Shader.PropertyToID("_Inverse");
        private static readonly int WARP_ANGLE_MIN = Shader.PropertyToID("_WarpAngleMin");
        private static readonly int WARP_ANGLE_MAX = Shader.PropertyToID("_WarpAngleMax");
        private static readonly int WARP_INTENSITY = Shader.PropertyToID("_WarpIntensity");
        private static readonly int TEXEL_SIZE = Shader.PropertyToID("_TexelSize");
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int REMAP_TEX = Shader.PropertyToID("_RemapTex");
        private static readonly int VORONOI_TEX = Shader.PropertyToID("_VoronoiTex");
        private static readonly int VORONOI_TEXEL_SIZE = Shader.PropertyToID("_VoronoiTex_TexelSize");

        private static readonly int PASS_NO_WARP = 0;
        private static readonly int PASS_ANGULAR_WARP = 1;
        private static readonly int PASS_DIRECTIONAL_WARP = 2;

        private static readonly int PASS_OUTPUT_CELLS = 3;
        private static readonly int PASS_OUTPUT_RAW_CELLS = 4;
        private static readonly int PASS_OUTPUT_OUTLINE = 5;

        private static readonly string TMP_RT_NAME = "VoronoiTempRT";

        private Material m_material;

        public VoronoiNode() : base()
        {
            m_offset = Vector2.zero;
            m_scale = 500;
            m_amplitudeExponent = 1;
            m_lacunarity = 2;
            m_persistence = 1f / 3f;
            m_layerCount = 4;

            m_warpMode = WarpMode.None;
            m_warpAngleMin = -360;
            m_warpAngleMax = 360;
            m_warpIntensity = 1;

            m_remapCurve = AnimationCurve.Linear(0, 0, 1, 1);

            System.Random rnd = new System.Random();
            m_seed = rnd.Next(0, 1000);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor tmpDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RGFloat);
            RenderTexture tempRt = context.CreateTemporaryRT(tmpDesc, TMP_RT_NAME);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetVector(OFFSET, m_offset);
            m_material.SetFloat(SCALE, m_scale);
            m_material.SetFloat(LACUNARITY, m_lacunarity);
            m_material.SetFloat(PERSISTENCE, m_persistence);
            m_material.SetFloat(AMPLITUDE_EXP, m_amplitudeExponent);
            m_material.SetInt(LAYER_COUNT, m_layerCount);
            m_material.SetFloat(INVERSE, m_inverse ? 1 : 0);
            m_material.SetVector(TEXTURE_SIZE, new Vector4(tempRt.width, tempRt.height, 0, 0));

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            m_material.SetVector(WORLD_BOUNDS, worldBounds);

            int baseSeed = context.GetArg(Args.SEED).intValue;
            System.Random rnd = new System.Random(baseSeed ^ m_seed);
            Vector2 randomOffset = new Vector2(rnd.Next(-100, 100), rnd.Next(-100, 100));
            m_material.SetVector(RANDOM_OFFSET, randomOffset);

            Texture2D remapTex = Utilities.TextureFromCurve(m_remapCurve);
            m_material.SetTexture(REMAP_TEX, remapTex);

            if (m_warpMode == WarpMode.Angular)
            {
                m_material.SetFloat(WARP_ANGLE_MIN, m_warpAngleMin * Mathf.Deg2Rad);
                m_material.SetFloat(WARP_ANGLE_MAX, m_warpAngleMax * Mathf.Deg2Rad);
                m_material.SetFloat(WARP_INTENSITY, m_warpIntensity);
                Drawing.DrawQuad(tempRt, m_material, PASS_ANGULAR_WARP);
            }
            else if (m_warpMode == WarpMode.Directional)
            {
                m_material.SetFloat(WARP_INTENSITY, m_warpIntensity);
                m_material.SetVector(TEXEL_SIZE, tempRt.texelSize);
                Drawing.DrawQuad(tempRt, m_material, PASS_DIRECTIONAL_WARP);
            }
            else
            {
                Drawing.DrawQuad(tempRt, m_material, PASS_NO_WARP);
            }

            Object.DestroyImmediate(remapTex);

            SlotRef cellsRef = new SlotRef(m_id, cellsSlot.id);
            if (context.GetReferenceCount(cellsRef) > 0 || context.IsTargetNode(m_id))
            {
                m_material.SetTexture(VORONOI_TEX, tempRt);
                DataPool.RtDescriptor cellDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
                RenderTexture cellsRt = context.CreateRenderTarget(cellDesc, cellsRef);
                Drawing.DrawQuad(cellsRt, m_material, PASS_OUTPUT_CELLS);
            }

            SlotRef rawCellsRef = new SlotRef(m_id, rawCellsSlot.id);
            if (context.GetReferenceCount(rawCellsRef) > 0)
            {
                m_material.SetTexture(VORONOI_TEX, tempRt);
                DataPool.RtDescriptor rawCellsDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
                RenderTexture rawCellsRt = context.CreateRenderTarget(rawCellsDesc, rawCellsRef);
                Drawing.DrawQuad(rawCellsRt, m_material, PASS_OUTPUT_RAW_CELLS);
            }

            SlotRef outlineRef = new SlotRef(m_id, outlineSlot.id);
            if (context.GetReferenceCount(outlineRef) > 0)
            {
                m_material.SetTexture(VORONOI_TEX, tempRt);
                m_material.SetVector(VORONOI_TEXEL_SIZE, tempRt.texelSize);
                DataPool.RtDescriptor outlineDesc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
                RenderTexture outlineRt = context.CreateRenderTarget(outlineDesc, outlineRef);
                Drawing.DrawQuad(outlineRt, m_material, PASS_OUTPUT_OUTLINE);
            }

            context.ReleaseTemporary(TMP_RT_NAME);
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
