#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Slope Mask",
        path = "Masking/Slope Mask",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ecw6sj7j7lq5",
        keywords = "slope, steepness, angle, select, normal",
        description = "Highlight the geometry that is in a specific slope angle.")]
    public class SlopeMaskNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_minAngle;
        public float minAngle
        {
            get
            {
                return m_minAngle;
            }
            set
            {
                m_minAngle = Mathf.Clamp(Mathf.Min(value, m_maxAngle), 0, 90);
            }
        }

        [SerializeField]
        private float m_maxAngle;
        public float maxAngle
        {
            get
            {
                return m_maxAngle;
            }
            set
            {
                m_maxAngle = Mathf.Clamp(Mathf.Max(value, m_minAngle), 0, 90);
            }
        }

        [SerializeField]
        private AnimationCurve m_transition;
        public AnimationCurve transition
        {
            get
            {
                return m_transition;
            }
            set
            {
                m_transition = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/SlopeMask";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MIN_ANGLE = Shader.PropertyToID("_MinAngle");
        private static readonly int MAX_ANGLE = Shader.PropertyToID("_MaxAngle");
        private static readonly int TRANSITION = Shader.PropertyToID("_Transition");
        private static readonly int TERRAIN_SIZE = Shader.PropertyToID("_TerrainSize");
        private static readonly int PASS = 0;

        private Material m_material;

        public SlopeMaskNode() : base()
        {
            m_minAngle = 0;
            m_maxAngle = 45;
            m_transition = Utilities.EaseInOutCurve();
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
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.RFloat);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            Texture2D transitionTexture = Utilities.TextureFromCurve(m_transition);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(MIN_ANGLE, m_minAngle * Mathf.Deg2Rad);
            m_material.SetFloat(MAX_ANGLE, m_maxAngle * Mathf.Deg2Rad);
            m_material.SetTexture(TRANSITION, transitionTexture);

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 terrainSize = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);
            m_material.SetVector(TERRAIN_SIZE, terrainSize);

            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(transitionTexture);
            Object.DestroyImmediate(m_material);
        }

        public override void Bypass(GraphContext context)
        {
            return;
        }
    }
}
#endif
