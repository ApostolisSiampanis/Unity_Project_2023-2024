#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Polygon Mask",
        path = "Masking/Polygon Mask",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.vx6vr9ljdvzo",
        keywords = "",
        description = "Draw a polygon defined by a set of points selected manually.")]
    public class PolygonMaskNode : ImageNodeBase
    {
        public readonly MaskSlot falloffSlot = new MaskSlot("Falloff", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private Vector2[] m_vertices;
        public Vector2[] vertices
        {
            get
            {
                return m_vertices;
            }
            set
            {
                m_vertices = value;
            }
        }

        [SerializeField]
        private float m_falloff;
        public float falloff
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

        public PolygonMaskNode() : base()
        {
            m_vertices = new Vector2[0];
            m_falloff = 0;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);
            if (m_vertices.Length < 3)
            {
                Drawing.Blit(Texture2D.blackTexture, targetRt);
                return;
            }

            SlotRef falloffRefLink = context.GetInputLink(m_id, falloffSlot.id);
            Texture falloffMap = context.GetTexture(falloffRefLink);

            PolygonMaskRenderer.Configs rendererConfigs = new PolygonMaskRenderer.Configs();
            rendererConfigs.vertices = m_vertices;
            rendererConfigs.falloffVertices = GetFalloffVertices();
            rendererConfigs.falloffTexture = falloffMap;

            PolygonMaskRenderer.Render(targetRt, rendererConfigs);

            context.ReleaseReference(falloffRefLink);
        }

        private Vector2[] GetFalloffVertices(float falloff)
        {
            if (m_vertices.Length < 3)
            {
                return new Vector2[0];
            }

            Vector2[] falloffVertices = new Vector2[m_vertices.Length];
            bool reverse = false;
            for (int i = 0; i < falloffVertices.Length; ++i)
            {
                Vector2[] segments = GetSegments(i);
                Vector2 dir0 = (segments[1] - segments[0]).normalized;
                Vector2 dir1 = (segments[2] - segments[1]).normalized;
                Vector2 dir = (dir0 + dir1).normalized;
                if (i == 0)
                {
                    Vector3 cross = Vector3.Cross(dir0, dir1);
                    reverse = cross.z >= 0;
                }
                Vector2 normal;
                if (reverse)
                {
                    normal = new Vector2(dir.y, -dir.x);
                }
                else
                {
                    normal = new Vector2(-dir.y, dir.x);
                }
                falloffVertices[i] = m_vertices[i] + normal * falloff;
            }
            return falloffVertices;
        }

        public Vector2[] GetFalloffVertices()
        {
            return GetFalloffVertices(m_falloff);
        }

        private Vector2[] GetSegments(int vertexIndex)
        {
            if (m_vertices.Length < 3)
                throw new System.Exception("Can't get segments, vertex count must >= 3");
            if (vertexIndex < 0 || vertexIndex >= m_vertices.Length)
                throw new System.Exception("Invalid vertex index");
            if (vertexIndex == 0)
            {
                return new Vector2[3]
                {
                    m_vertices[m_vertices.Length - 1],
                    m_vertices[0],
                    m_vertices[1]
                };
            }
            else if (vertexIndex == m_vertices.Length - 1)
            {
                return new Vector2[3]
                {
                    m_vertices[m_vertices.Length - 2],
                    m_vertices[m_vertices.Length - 1],
                    m_vertices[0]
                };
            }
            else
            {
                return new Vector2[3]
                {
                    m_vertices[vertexIndex - 1],
                    m_vertices[vertexIndex],
                    m_vertices[vertexIndex + 1]
                };
            }
        }

        public override void Bypass(GraphContext context)
        {
            return;
        }
    }
}
#endif
