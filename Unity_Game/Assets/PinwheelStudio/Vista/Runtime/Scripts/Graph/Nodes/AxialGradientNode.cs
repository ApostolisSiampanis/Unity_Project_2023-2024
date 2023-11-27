#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Axial Gradient",
        path = "Base Shape/Axial Gradient",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ui6tjfple7mj",
        keywords = "",
        description = "Generate a simple axial gradient")]
    public class AxialGradientNode : ImageNodeBase
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

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/AxialGradient";
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int UV_TO_LINE_MATRIX = Shader.PropertyToID("_UvToLineMatrix");
        private static readonly int PASS = 0;

        private static readonly string KW_REPEAT = "REPEAT";

        private Material m_material;

        public AxialGradientNode() : base()
        {
            m_startPoint = Vector2.zero;
            m_endPoint = Vector2.one;
            m_useWorldPosition = false;
            m_repeat = false;
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
            float d = Vector3.Distance(start, end);
            Vector3 scale = new Vector3(d, d, 1);
            Matrix4x4 uvToLineMatrix = Matrix4x4.TRS(pos, rotation, scale).inverse;
            m_material.SetMatrix(UV_TO_LINE_MATRIX, uvToLineMatrix);

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
