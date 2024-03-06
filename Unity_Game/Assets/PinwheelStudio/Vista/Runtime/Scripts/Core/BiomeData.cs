#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public class BiomeData : System.IDisposable
    {
        public RenderTexture heightMap { get; set; }
        public RenderTexture holeMap { get; set; }
        public RenderTexture meshDensityMap { get; set; }
        public RenderTexture albedoMap { get; set; }
        public RenderTexture metallicMap { get; set; }

        internal Collector<TerrainLayer> m_terrainLayers;
        internal Collector<RenderTexture> m_layerWeights;

        internal Collector<TreeTemplate> m_treeTemplates;
        internal Collector<ComputeBuffer> m_treeBuffers;

        internal Collector<DetailTemplate> m_detailTemplates_Density;
        internal Collector<RenderTexture> m_detailDensityMaps;

        internal Collector<DetailTemplate> m_detailTemplates_Instances;
        internal Collector<ComputeBuffer> m_detailInstanceBuffers;

        internal Collector<ObjectTemplate> m_objectTemplates;
        internal Collector<ComputeBuffer> m_objectBuffers;

        internal Collector<string> m_genericTextureLabels;
        internal Collector<RenderTexture> m_genericTextures;

        internal Collector<string> m_genericBufferLabels;
        internal Collector<ComputeBuffer> m_genericBuffers;

        public RenderTexture biomeMaskMap { get; set; }

        public BiomeData()
        {
            m_terrainLayers = new Collector<TerrainLayer>();
            m_layerWeights = new Collector<RenderTexture>();

            m_treeTemplates = new Collector<TreeTemplate>();
            m_treeBuffers = new Collector<ComputeBuffer>();

            m_detailTemplates_Density = new Collector<DetailTemplate>();
            m_detailDensityMaps = new Collector<RenderTexture>();

            m_detailTemplates_Instances = new Collector<DetailTemplate>();
            m_detailInstanceBuffers = new Collector<ComputeBuffer>();

            m_objectTemplates = new Collector<ObjectTemplate>();
            m_objectBuffers = new Collector<ComputeBuffer>();

            m_genericTextureLabels = new Collector<string>();
            m_genericTextures = new Collector<RenderTexture>();

            m_genericBufferLabels = new Collector<string>();
            m_genericBuffers = new Collector<ComputeBuffer>();
        }

        public void AddTextureLayer(TerrainLayer desc, RenderTexture texture)
        {
            m_terrainLayers.Add(desc);
            m_layerWeights.Add(texture);
        }

        public void GetLayerWeights(List<TerrainLayer> layers, List<RenderTexture> weights)
        {
            layers.Clear();
            layers.AddRange(m_terrainLayers);
            weights.Clear();
            weights.AddRange(m_layerWeights);
        }

        public int GetLayerCount()
        {
            if (m_layerWeights != null)
            {
                return m_layerWeights.Count;
            }
            else
            {
                return 0;
            }
        }

        public void AddTree(TreeTemplate template, ComputeBuffer buffer)
        {
            m_treeTemplates.Add(template);
            m_treeBuffers.Add(buffer);
        }

        public void GetTrees(List<TreeTemplate> templates, List<ComputeBuffer> buffers)
        {
            templates.Clear();
            templates.AddRange(m_treeTemplates);
            buffers.Clear();
            buffers.AddRange(m_treeBuffers);
        }

        public void AddObject(ObjectTemplate template, ComputeBuffer buffer)
        {
            m_objectTemplates.Add(template);
            m_objectBuffers.Add(buffer);
        }

        public void GetObjects(List<ObjectTemplate> templates, List<ComputeBuffer> buffers)
        {
            templates.Clear();
            templates.AddRange(m_objectTemplates);
            buffers.Clear();
            buffers.AddRange(m_objectBuffers);
        }

        public void AddDetailDensity(DetailTemplate template, RenderTexture densityMap)
        {
            m_detailTemplates_Density.Add(template);
            m_detailDensityMaps.Add(densityMap);
        }

        public void GetDensityMaps(List<DetailTemplate> templates, List<RenderTexture> densityMaps)
        {
            templates.Clear();
            templates.AddRange(m_detailTemplates_Density);
            densityMaps.Clear();
            densityMaps.AddRange(m_detailDensityMaps);
        }

        public void AddDetailInstance(DetailTemplate template, ComputeBuffer buffer)
        {
            m_detailTemplates_Instances.Add(template);
            m_detailInstanceBuffers.Add(buffer);
        }

        public void GetDetailInstances(List<DetailTemplate> templates, List<ComputeBuffer> buffers)
        {
            templates.Clear();
            templates.AddRange(m_detailTemplates_Instances);
            buffers.Clear();
            buffers.AddRange(m_detailInstanceBuffers);
        }

        public void AddGenericTexture(string label, RenderTexture texture)
        {
            m_genericTextureLabels.Add(label);
            m_genericTextures.Add(texture);
        }

        public void GetGenericTextures(List<string> labels, List<RenderTexture> textures)
        {
            labels.Clear();
            labels.AddRange(m_genericTextureLabels);
            textures.Clear();
            textures.AddRange(m_genericTextures);
        }

        public void AddGenericBuffer(string label, ComputeBuffer buffer)
        {
            m_genericBufferLabels.Add(label);
            m_genericBuffers.Add(buffer);
        }

        public void GetGenericBuffers(List<string> labels, List<ComputeBuffer> buffers)
        {
            labels.Clear();
            labels.AddRange(m_genericBufferLabels);
            buffers.Clear();
            buffers.AddRange(m_genericBuffers);
        }

        public void Dispose()
        {
            if (heightMap != null)
            {
                heightMap.Release();
                Object.DestroyImmediate(heightMap);
                heightMap = null;
            }
            if (holeMap != null)
            {
                holeMap.Release();
                Object.DestroyImmediate(holeMap);
                holeMap = null;
            }
            if (meshDensityMap != null)
            {
                meshDensityMap.Release();
                Object.DestroyImmediate(meshDensityMap);
                meshDensityMap = null;
            }
            if (albedoMap != null)
            {
                albedoMap.Release();
                Object.DestroyImmediate(albedoMap);
                albedoMap = null;
            }
            if (metallicMap != null)
            {
                metallicMap.Release();
                Object.DestroyImmediate(metallicMap);
                metallicMap = null;
            }
            foreach (RenderTexture t in m_layerWeights)
            {
                if (t != null)
                {
                    t.Release();
                    Object.DestroyImmediate(t);
                }
            }
            m_terrainLayers.Clear();
            m_layerWeights.Clear();

            foreach (ComputeBuffer b in m_treeBuffers)
            {
                if (b != null)
                {
                    b.Release();
                }
            }
            m_treeTemplates.Clear();
            m_treeBuffers.Clear();

            foreach (ComputeBuffer b in m_objectBuffers)
            {
                if (b != null)
                {
                    b.Release();
                }
            }
            m_objectTemplates.Clear();
            m_objectBuffers.Clear();

            foreach (RenderTexture t in m_detailDensityMaps)
            {
                if (t != null)
                {
                    t.Release();
                    Object.DestroyImmediate(t);
                }
            }
            m_detailTemplates_Density.Clear();
            m_detailDensityMaps.Clear();

            foreach (ComputeBuffer b in m_detailInstanceBuffers)
            {
                if (b != null)
                {
                    b.Release();
                }
            }
            m_detailTemplates_Instances.Clear();
            m_detailInstanceBuffers.Clear();

            foreach (RenderTexture t in m_genericTextures)
            {
                if (t != null)
                {
                    t.Release();
                    Object.DestroyImmediate(t);
                }
            }
            m_genericTextureLabels.Clear();
            m_genericTextures.Clear();

            foreach (ComputeBuffer b in m_genericBuffers)
            {
                if (b != null)
                {
                    b.Release();
                }
            }
            m_genericBufferLabels.Clear();
            m_genericBuffers.Clear();

            if (biomeMaskMap != null)
            {
                biomeMaskMap.Release();
                Object.DestroyImmediate(biomeMaskMap);
                biomeMaskMap = null;
            }
        }
    }
}
#endif
