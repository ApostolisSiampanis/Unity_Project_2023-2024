#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Noise",
        path = "Base Shape/Noise",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.gmzo134ddw49",
        keywords = "perlin, billow, ridge, noise",
        description = "Generate fractal noise, useful for basic mountain shape.\n<ss>Tips: Search for the noise mode directly (eg: perlin, billow, ridge, etc.).</ss>")]
    public class NoiseNode : ImageNodeBase, ISetupWithHint, IHasSeed
    {
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

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
        private NoiseMode m_mode;
        public NoiseMode mode
        {
            get
            {
                return m_mode;
            }
            set
            {
                m_mode = value;
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

        private Material m_material;

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Noise";
        private static readonly int WORLD_BOUNDS = Shader.PropertyToID("_WorldBounds");
        private static readonly int OFFSET = Shader.PropertyToID("_Offset");
        private static readonly int SCALE = Shader.PropertyToID("_Scale");
        private static readonly int LACUNARITY = Shader.PropertyToID("_Lacunarity");
        private static readonly int PERSISTENCE = Shader.PropertyToID("_Persistence");
        private static readonly int LAYER_COUNT = Shader.PropertyToID("_LayerCount");
        private static readonly int RANDOM_OFFSET = Shader.PropertyToID("_RandomOffset");
        private static readonly int NOISE_TYPE = Shader.PropertyToID("_NoiseType");
        private static readonly int WARP_ANGLE_MIN = Shader.PropertyToID("_WarpAngleMin");
        private static readonly int WARP_ANGLE_MAX = Shader.PropertyToID("_WarpAngleMax");
        private static readonly int WARP_INTENSITY = Shader.PropertyToID("_WarpIntensity");
        private static readonly int TEXEL_SIZE = Shader.PropertyToID("_TexelSize");
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int REMAP_TEX = Shader.PropertyToID("_RemapTex");
        private static readonly int SEED = Shader.PropertyToID("_Seed");

        private static readonly int PASS_NO_WARP = 0;
        private static readonly int PASS_ANGULAR_WARP = 1;
        private static readonly int PASS_DIRECTIONAL_WARP = 2;

        public NoiseNode() : base()
        {
            m_offset = Vector2.zero;
            m_scale = 500;
            m_lacunarity = 2;
            m_persistence = 1f / 3f;
            m_layerCount = 4;
            m_mode = NoiseMode.Perlin01;

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
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetVector(OFFSET, m_offset);
            m_material.SetFloat(SCALE, m_scale);
            m_material.SetFloat(LACUNARITY, m_lacunarity);
            m_material.SetFloat(PERSISTENCE, m_persistence);
            m_material.SetInt(LAYER_COUNT, m_layerCount);
            m_material.SetInt(NOISE_TYPE, (int)m_mode);
            m_material.SetVector(TEXTURE_SIZE, new Vector4(targetRt.width, targetRt.height, 0, 0));

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            m_material.SetVector(WORLD_BOUNDS, worldBounds);

            int baseSeed = context.GetArg(Args.SEED).intValue;
            System.Random rnd = new System.Random(baseSeed ^ m_seed);
            Vector2 randomOffset = new Vector2(rnd.Next(-1000, 1000), rnd.Next(-1000, 1000));
            m_material.SetVector(RANDOM_OFFSET, randomOffset);
            m_material.SetInt(SEED, baseSeed ^ m_seed);

            Texture2D remapTex = Utilities.TextureFromCurve(m_remapCurve);
            m_material.SetTexture(REMAP_TEX, remapTex);

            if (m_warpMode == WarpMode.Angular)
            {
                m_material.SetFloat(WARP_ANGLE_MIN, m_warpAngleMin * Mathf.Deg2Rad);
                m_material.SetFloat(WARP_ANGLE_MAX, m_warpAngleMax * Mathf.Deg2Rad);
                m_material.SetFloat(WARP_INTENSITY, m_warpIntensity);
                Drawing.DrawQuad(targetRt, m_material, PASS_ANGULAR_WARP);
            }
            else if (m_warpMode == WarpMode.Directional)
            {
                m_material.SetFloat(WARP_INTENSITY, m_warpIntensity);
                m_material.SetVector(TEXEL_SIZE, targetRt.texelSize);
                Drawing.DrawQuad(targetRt, m_material, PASS_DIRECTIONAL_WARP);
            }
            else
            {
                Drawing.DrawQuad(targetRt, m_material, PASS_NO_WARP);
            }

            Object.DestroyImmediate(remapTex);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public void SetupWithHint(string hint)
        {
            if (hint.StartsWith("perlin"))
            {
                mode = NoiseMode.Perlin01;
            }
            else if (hint.StartsWith("billow"))
            {
                mode = NoiseMode.Billow;
            }
            else if (hint.StartsWith("ridge"))
            {
                mode = NoiseMode.Ridged;
            }
        }
    }
}
#endif
