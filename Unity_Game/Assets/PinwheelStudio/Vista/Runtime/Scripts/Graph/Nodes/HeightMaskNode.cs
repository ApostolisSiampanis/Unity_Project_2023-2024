#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Height Mask",
        path = "Masking/Height Mask",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.jze9g5nygz1y",
        keywords = "height, select, altitude, elevation",
        description = "Highlight the area which is in a specific height range.")]
    public class HeightMaskNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

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
                m_minHeight = Mathf.Max(0, Mathf.Min(value, m_maxHeight));
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
                m_maxHeight = Mathf.Max(0, Mathf.Max(value, m_minHeight));
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

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/HeightMask";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MIN_HEIGHT = Shader.PropertyToID("_MinHeight");
        private static readonly int MAX_HEIGHT = Shader.PropertyToID("_MaxHeight");
        private static readonly int TRANSITION = Shader.PropertyToID("_Transition");
        private static readonly int TERRAIN_SIZE = Shader.PropertyToID("_TerrainSize");
        private static readonly int PASS = 0;

        private Material m_material;

        public HeightMaskNode() : base()
        {
            m_minHeight = 0;
            m_maxHeight = 100;
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
            m_material.SetFloat(MIN_HEIGHT, m_minHeight);
            m_material.SetFloat(MAX_HEIGHT, m_maxHeight);
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
