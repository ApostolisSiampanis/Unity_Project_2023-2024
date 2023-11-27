#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.Graph;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class TerrainSubGraphInputProvider : IExternalInputProvider
    {
        [SerializeField]
        private TerrainGraphEditor m_parentGraphEditor;
        [SerializeField]
        private string m_subGraphNodeId;

        private List<GraphRenderTexture> m_textures;
        private List<GraphBuffer> m_buffers;

        public TerrainSubGraphInputProvider(TerrainGraphEditor parentGraphEditor, string subGraphNodeId)
        {
            m_parentGraphEditor = parentGraphEditor;
            m_subGraphNodeId = subGraphNodeId;
        }

        public void SetInput(GraphInputContainer inputContainer)
        {
            m_textures = new List<GraphRenderTexture>();
            m_buffers = new List<GraphBuffer>();

            TerrainSubGraphNode.gatherInputTextureCallback = OnGatherInputTexture;
            TerrainSubGraphNode.gatherInputBufferCallback = OnGatherInputBuffer;

            DataPool data = m_parentGraphEditor.ExecuteGraphImmediateAt(m_subGraphNodeId);
            data.Dispose();

            foreach (GraphRenderTexture rt in m_textures)
            {
                inputContainer.AddTexture(rt.identifier, rt.renderTexture);
            }
            foreach (GraphBuffer b in m_buffers)
            {
                inputContainer.AddBuffer(b.identifier, b.buffer);
            }
        }

        private void OnGatherInputTexture(TerrainSubGraphNode sender, string name, RenderTexture inputTexture)
        {
            if (!string.Equals(sender.id, m_subGraphNodeId))
                return;

            GraphRenderTexture graphRT = new GraphRenderTexture(inputTexture.width, inputTexture.height, inputTexture.format);
            graphRT.identifier = name;
            Drawing.Blit(inputTexture, graphRT);

            m_textures.Add(graphRT);
        }

        private void OnGatherInputBuffer(TerrainSubGraphNode sender, string name, ComputeBuffer inputBuffer)
        {
            if (!string.Equals(sender.id, m_subGraphNodeId))
                return;

            GraphBuffer graphBuffer = new GraphBuffer(inputBuffer.count, inputBuffer.stride);
            graphBuffer.identifier = name;
            BufferHelper.Copy(inputBuffer, graphBuffer.buffer);

            m_buffers.Add(graphBuffer);
        }

        public void CleanUp()
        {
            foreach (GraphRenderTexture rt in m_textures)
            {
                if (rt != null)
                {
                    rt.Dispose();
                }
            }
            m_textures = null;

            foreach (GraphBuffer b in m_buffers)
            {
                if (b != null)
                {
                    b.Dispose();
                }
            }
            m_buffers = null;
        }
    }
}
#endif
