#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Radial Gradient",
        path = "Base Shape/Radial Gradient",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.v7jpmjkfen0p",
        keywords = "",
        description = "Generate a simple radial gradient.")]
    public class RadialGradientNode : ImageNodeBase
    {
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private Vector2 m_center;
        public Vector2 center
        {
            get
            {
                return m_center;
            }
            set
            {
                m_center = value;
            }
        }

        [SerializeField]
        private float m_radius;
        public float radius
        {
            get
            {
                return m_radius;
            }
            set
            {
                m_radius = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool m_useWorldPosition;
        public bool useWorldPosition
        {
            get
            {
                return m_useWorldPosition;
            }
            set
            {
                m_useWorldPosition = value;
            }
        }

        [SerializeField]
        private bool m_repeat;
        public bool repeat
        {
            get
            {
                return m_repeat;
            }
            set
            {
                m_repeat = value;
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/RadialGradient";
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int CENTER = Shader.PropertyToID("_Center");
        private static readonly int RADIUS = Shader.PropertyToID("_Radius");
        private static readonly int PASS = 0;

        private static readonly string KW_REPEAT = "REPEAT";

        private Material m_material;

        public RadialGradientNode() : base()
        {
            m_center = Vector2.one * 0.5f;
            m_radius = 0.5f;
            m_useWorldPosition = false;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetVector(TEXTURE_SIZE, new Vector4(targetRt.width, targetRt.height, 0, 0));

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            Vector2 c = m_center;
            float r = m_radius;
            if (m_useWorldPosition)
            {
                c.x = Utilities.InverseLerpUnclamped(worldBounds.x, worldBounds.x + worldBounds.z, c.x);
                c.y = Utilities.InverseLerpUnclamped(worldBounds.y, worldBounds.y + worldBounds.w, c.y);

                r = r / worldBounds.z;
            }

            m_material.SetVector(CENTER, c);
            m_material.SetFloat(RADIUS, r);

            if (m_repeat)
            {
                m_material.EnableKeyword(KW_REPEAT);
            }
            else
            {
                m_material.DisableKeyword(KW_REPEAT);
            }

            Drawing.DrawQuad(targetRt, m_material, PASS);
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
