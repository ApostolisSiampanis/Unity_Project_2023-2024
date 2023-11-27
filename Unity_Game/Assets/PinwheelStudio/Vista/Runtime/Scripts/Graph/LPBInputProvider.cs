#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class LPBInputProvider : IExternalInputProvider, ITerrainGraphArgumentsFiller//, ISerializationCallbackReceiver
    {
        private LocalProceduralBiome m_biome;
        private LocalProceduralBiome biome
        {
            get
            {
                if (m_biome == null)
                {
                    foreach (LocalProceduralBiome b in LocalProceduralBiome.allInstances)
                    {
                        if (b.m_guid == m_biomeInstanceGuid)
                        {
                            m_biome = b;
                            break;
                        }
                    }
                }
                return m_biome;
            }
        }

        [SerializeField]
        private string m_biomeInstanceGuid;

        private List<GraphRenderTexture> m_textures;
        private List<GraphBuffer> m_buffers;

        public LPBInputProvider(LocalProceduralBiome b)
        {
            m_biome = b;
            m_biomeInstanceGuid = b.m_guid;
        }

        public void SetInput(GraphInputContainer inputContainer)
        {
            m_textures = new List<GraphRenderTexture>();
            m_buffers = new List<GraphBuffer>();

            GraphRenderTexture combinedBiomeMask = biome.RenderCombinedBiomeMask();
            combinedBiomeMask.identifier = GraphConstants.BIOME_MASK_INPUT_NAME;
            m_textures.Add(combinedBiomeMask);

            GraphRenderTexture sceneHeightMap = null;
            if (biome.shouldCollectSceneHeight)
            {
                sceneHeightMap = biome.RenderSceneHeightMap();
                sceneHeightMap.identifier = GraphConstants.SCENE_HEIGHT_INPUT_NAME;
                m_textures.Add(sceneHeightMap);
            }

            GraphRenderTexture[] customInputRT = CopyTextureInputsToRT();
            foreach (GraphRenderTexture rt in customInputRT)
            {
                if (rt != null)
                {
                    m_textures.Add(rt);
                }
            }

            GraphBuffer[] positionInputsBuffers = CopyPositionInputsToBuffers();
            foreach (GraphBuffer b in positionInputsBuffers)
            {
                if (b != null)
                {
                    m_buffers.Add(b);
                }
            }

            foreach (GraphRenderTexture rt in m_textures)
            {
                inputContainer.AddTexture(rt.identifier, rt.renderTexture);
            }
            foreach (GraphBuffer b in m_buffers)
            {
                inputContainer.AddBuffer(b.identifier, b.buffer);
            }
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

        internal GraphRenderTexture[] CopyTextureInputsToRT()
        {
            TextureInput[] textureInputs = biome.textureInputs;
            GraphRenderTexture[] renderTextures = new GraphRenderTexture[textureInputs.Length];
            for (int i = 0; i < textureInputs.Length; ++i)
            {
                TextureInput input = textureInputs[i];
                if (input == null ||
                    input.texture == null ||
                    string.IsNullOrEmpty(input.name))
                    continue;

                Texture2D t2d = input.texture;
                bool isRawTexture =
                    t2d.format == TextureFormat.R8 ||
                    t2d.format == TextureFormat.R16 ||
                    t2d.format == TextureFormat.RFloat;
                RenderTextureFormat rtFormat = isRawTexture ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGB32;
                GraphRenderTexture rt = new GraphRenderTexture(t2d.width, t2d.height, rtFormat);
                rt.identifier = input.name;
                Drawing.Blit(t2d, rt);
                renderTextures[i] = rt;
            }
            return renderTextures;
        }

        public GraphRenderTexture RemoveTexture(string identifier)
        {
            GraphRenderTexture rt = m_textures.Find(t => t.identifier.Equals(identifier));
            if (rt != null)
            {
                m_textures.Remove(rt);
                return rt;
            }
            else
            {
                return null;
            }
        }

        internal GraphBuffer[] CopyPositionInputsToBuffers()
        {
            PositionInput[] positionInputs = biome.positionInputs;
            GraphBuffer[] buffers = new GraphBuffer[positionInputs.Length];
            for (int i = 0; i < positionInputs.Length; ++i)
            {
                PositionInput input = positionInputs[i];
                if (input == null ||
                    input.positionContainer == null ||
                    input.positionContainer == null ||
                    input.positionContainer.positions.Length == 0)
                    continue;

                GraphBuffer b = new GraphBuffer(input.positionContainer.positions.Length * PositionSample.SIZE, sizeof(float));
                b.identifier = input.name;
                b.buffer.SetData(input.positionContainer.positions);
                buffers[i] = b;
            }
            return buffers;
        }

        public void FillTerrainGraphArguments(TerrainGraph graph, IDictionary<int, Args> args)
        {
            args.Add(Args.BIOME_SCALE, Args.Create(biome.transform.localScale));
            args.Add(Args.BIOME_SPACE, Args.Create((int)biome.space));

            Bounds biomeWorldBounds = biome.worldBounds;
            Vector4 bwbArg = new Vector4(biomeWorldBounds.min.x, biomeWorldBounds.min.z, biomeWorldBounds.size.x, biomeWorldBounds.size.z);
            args.Add(Args.BIOME_WORLD_BOUNDS, Args.Create(bwbArg));
        }

        public void OnBeforeSerialize()
        {
            m_biomeInstanceGuid = m_biome.m_guid;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
#endif
