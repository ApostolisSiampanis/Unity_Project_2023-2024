#if VISTA
using UnityEngine;
using System;

namespace Pinwheel.Vista.Graph
{
    public class GraphRenderTexture : IDisposable
    {
        private string m_identifier;
        public string identifier
        {
            get
            {
                if (string.IsNullOrEmpty(m_identifier))
                {
                    m_identifier = string.Empty;
                }
                return m_identifier;
            }
            set
            {
                m_identifier = value;
            }
        }

        private RenderTexture m_renderTexture;
        public RenderTexture renderTexture
        {
            get
            {
                return m_renderTexture;
            }
        }

        private GraphRenderTexture()
        {

        }

        public GraphRenderTexture(int width, int height, RenderTextureFormat format)
        {
            m_renderTexture = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        }

        public override string ToString()
        {
            return identifier;
        }

        public void Dispose()
        {
            if (m_renderTexture != null)
            {
                m_renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_renderTexture);
            }
        }

        public static implicit operator RenderTexture(GraphRenderTexture graphRT)
        {
            if (graphRT == null)
                return null;
            else
                return graphRT.renderTexture;
        }

        public static implicit operator GraphRenderTexture(RenderTexture rt)
        {
            if (rt == null)
            {
                return null;
            }
            else
            {
                GraphRenderTexture graphRt = new GraphRenderTexture();
                graphRt.m_identifier = rt.name;
                graphRt.m_renderTexture = rt;
                return graphRt;
            }
        }
    }
}
#endif
