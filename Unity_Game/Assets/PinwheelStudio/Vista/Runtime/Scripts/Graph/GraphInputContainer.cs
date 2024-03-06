#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public class GraphInputContainer
    {
        private Dictionary<string, RenderTexture> m_textures;
        private Dictionary<string, ComputeBuffer> m_buffers;

        public GraphInputContainer()
        {
            m_textures = new Dictionary<string, RenderTexture>();
            m_buffers = new Dictionary<string, ComputeBuffer>();
        }

        public void AddTexture(string name, RenderTexture texture)
        {
            m_textures.Add(name, texture);
        }

        public void AddBuffer(string name, ComputeBuffer buffer)
        {
            m_buffers.Add(name, buffer);
        }

        internal void Bind(ref GraphContext context)
        {
            foreach (string k in m_textures.Keys)
            {
                context.AddExternalTexture(k, m_textures[k]);
            }
            foreach (string k in m_buffers.Keys)
            {
                context.AddExternalBuffer(k, m_buffers[k]);
            }
        }
    }
}
#endif
