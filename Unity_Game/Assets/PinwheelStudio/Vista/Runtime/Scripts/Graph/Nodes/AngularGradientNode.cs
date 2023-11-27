#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Angular Gradient",
        path = "Base Shape/Angular Gradient",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.1v2lncsua4cn",
        keywords = "",
        description = "Generate a simple angular gradient.")]
    public class AngularGradientNode : ImageNodeBase
    {
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private Vector2 m_startPoint;
        public Vector2 startPoint
        {
            get
            {
                return m_startPoint;
            }
            set
            {
                m_startPoint = value;
            }
        }

        [SerializeField]
        private Vector2 m_endPoint;
        public Vector2 endPoint
        {
            get
            {
                return m_endPoint;
            }
            set
            {
                m_endPoint = value;
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

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/AngularGradient";
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int TRANSFORM_MATRIX = Shader.PropertyToID("_TransformMatrix");
        private static readonly int PASS = 0;

        private Material m_material;

        public AngularGradientNode() : base()
        {
            m_startPoint = Vector2.one * 0.5f;
            m_endPoint = new Vector2(1f, 0.5f);
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
            Vector2 start = m_startPoint;
            Vector2 end = m_endPoint;
            if (m_useWorldPosition)
            {
                start.x = Utilities.InverseLerpUnclamped(worldBounds.x, worldBounds.x + worldBounds.z, start.x);
                start.y = Utilities.InverseLerpUnclamped(worldBounds.y, worldBounds.y + worldBounds.w, start.y);

                end.x = Utilities.InverseLerpUnclamped(worldBounds.x, worldBounds.x + worldBounds.z, end.x);
                end.y = Utilities.InverseLerpUnclamped(worldBounds.y, worldBounds.y + worldBounds.w, end.y);
            }

            Vector3 pos = start;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, Vector3.Normalize(end - start));
            Vector3 scale = Vector3.one;
            Matrix4x4 transformMatrix = Matrix4x4.TRS(pos, rotation, scale).inverse;
            m_material.SetMatrix(TRANSFORM_MATRIX, transformMatrix);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
