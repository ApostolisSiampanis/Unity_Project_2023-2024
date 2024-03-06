#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Direction Mask",
        path = "Masking/Direction Mask",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.8b8w5glvo6xg",
        keywords = "",
        description = "Highlight the geometry where it's normal vector points toward a specific angle range.\nEx: Having different texture for the north & south region of a mountain.")]
    public class DirectionMaskNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_angle;
        public float angle
        {
            get
            {
                return m_angle;
            }
            set
            {
                m_angle = Mathf.Clamp(value, 0, 360);
            }
        }

        [SerializeField]
        private float m_tolerance;
        public float tolerance
        {
            get
            {
                return m_tolerance;
            }
            set
            {
                m_tolerance = Mathf.Clamp(value, 0, 180);
            }
        }

        [SerializeField]
        private AnimationCurve m_falloff;
        public AnimationCurve falloff
        {
            get
            {
                return m_falloff;
            }
            set
            {
                m_falloff = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/DirectionMask";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int ANGLE = Shader.PropertyToID("_Angle");
        private static readonly int TOLERANCE = Shader.PropertyToID("_Tolerance");
        private static readonly int TERRAIN_SIZE = Shader.PropertyToID("_TerrainSize");
        private static readonly int FALLOFF_TEX = Shader.PropertyToID("_FalloffTex");
        private static readonly int PASS = 0;

        private Material m_material;

        public DirectionMaskNode() : base()
        {
            m_angle = 0;
            m_tolerance = 10;
            m_falloff = AnimationCurve.Linear(0, 0, 1, 1);
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

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(ANGLE, m_angle);
            m_material.SetFloat(TOLERANCE, m_tolerance);

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 terrainSize = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);
            m_material.SetVector(TERRAIN_SIZE, terrainSize);

            Texture2D falloffTex = Utilities.TextureFromCurve(m_falloff);
            m_material.SetTexture(FALLOFF_TEX, falloffTex);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);

            Object.DestroyImmediate(falloffTex);
            Object.DestroyImmediate(m_material);
        }

        public override void Bypass(GraphContext context)
        {
            return;
        }
    }
}
#endif
