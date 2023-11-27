#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public class GraphBuffer : IDisposable //Can't derive from the sealed ComputeBuffer
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

        private ComputeBuffer m_buffer;
        public ComputeBuffer buffer
        {
            get
            {
                return m_buffer;
            }
        }

        public GraphBuffer(int count, int stride)
        {
            m_buffer = new ComputeBuffer(count, stride);
        }

        public void Dispose()
        {
            if (m_buffer != null)
            {
                m_buffer.Dispose();
            }
        }

        public override string ToString()
        {
            return identifier;
        }
    }
}
#endif
