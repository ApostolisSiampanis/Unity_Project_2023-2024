#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class BiomeDataUtilities
    {
        private static readonly string BLIT_SHADER_NAME = "Hidden/Vista/BiomeDataBlit";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int RENDER_TARGET_SIZE = Shader.PropertyToID("_RenderTargetSize");
        private static readonly int SRC_BOUNDS = Shader.PropertyToID("_SrcBounds");
        private static readonly int DEST_BOUNDS = Shader.PropertyToID("_DestBounds");
        private static readonly int PASS = 0;

        public static void Copy(BiomeData srcData, Bounds srcBounds, BiomeData destData, Bounds destBounds, int heightMapResolution, int textureResolution)
        {
            destData.Dispose();
            Material mat = new Material(ShaderUtilities.Find(BLIT_SHADER_NAME));
            mat.SetVector(SRC_BOUNDS, new Vector4(srcBounds.min.x, srcBounds.min.z, srcBounds.size.x, srcBounds.size.z));
            mat.SetVector(DEST_BOUNDS, new Vector4(destBounds.min.x, destBounds.min.z, destBounds.size.x, destBounds.size.z));

            RenderTexture srcHeightMap = srcData.heightMap;
            if (srcHeightMap != null)
            {
                RenderTextureDescriptor desc = srcHeightMap.descriptor;
                desc.width = heightMapResolution;
                desc.height = heightMapResolution;
                RenderTexture destHeightMap = new RenderTexture(desc);
                CopyTexture(mat, srcHeightMap, destHeightMap);
                destData.heightMap = destHeightMap;
            }

            RenderTexture srcHoleMap = srcData.holeMap;
            if (srcHoleMap != null)
            {
                RenderTextureDescriptor desc = srcHoleMap.descriptor;
                desc.width = heightMapResolution;
                desc.height = heightMapResolution;
                RenderTexture destHoleMap = new RenderTexture(desc);
                CopyTexture(mat, srcHoleMap, destHoleMap);
                destData.holeMap = destHoleMap;
            }

            RenderTexture srcMeshDensityMap = srcData.meshDensityMap;
            if (srcMeshDensityMap != null)
            {
                RenderTextureDescriptor desc = srcMeshDensityMap.descriptor;
                desc.width = heightMapResolution;
                desc.height = heightMapResolution;
                RenderTexture destMeshDensityMap = new RenderTexture(desc);
                CopyTexture(mat, srcMeshDensityMap, destMeshDensityMap);
                destData.meshDensityMap = destMeshDensityMap;
            }

            RenderTexture srcAlbedoMap = srcData.albedoMap;
            if (srcAlbedoMap != null)
            {
                RenderTextureDescriptor desc = srcAlbedoMap.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destAlbedoMap = new RenderTexture(desc);
                CopyTexture(mat, srcAlbedoMap, destAlbedoMap);
                destData.albedoMap = destAlbedoMap;
            }

            RenderTexture srcMetallicMap = srcData.metallicMap;
            if (srcMetallicMap != null)
            {
                RenderTextureDescriptor desc = srcMetallicMap.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destMetallicMap = new RenderTexture(desc);
                CopyTexture(mat, srcMetallicMap, destMetallicMap);
                destData.metallicMap = destMetallicMap;
            }

            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
            List<RenderTexture> srcWeights = new List<RenderTexture>();
            srcData.GetLayerWeights(terrainLayers, srcWeights);
            for (int i = 0; i < terrainLayers.Count; ++i)
            {
                RenderTexture srcWeight = srcWeights[i];
                RenderTextureDescriptor desc = srcWeight.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destWeight = new RenderTexture(desc);
                CopyTexture(mat, srcWeight, destWeight);
                destData.AddTextureLayer(terrainLayers[i], destWeight);
            }

            List<DetailTemplate> detailTemplates_Density = new List<DetailTemplate>();
            List<RenderTexture> srcDensityMaps = new List<RenderTexture>();
            srcData.GetDensityMaps(detailTemplates_Density, srcDensityMaps);
            for (int i = 0; i < detailTemplates_Density.Count; ++i)
            {
                RenderTexture srcDensity = srcDensityMaps[i];
                RenderTextureDescriptor desc = srcDensity.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destDensity = new RenderTexture(desc);
                CopyTexture(mat, srcDensity, destDensity);
                destData.AddDetailDensity(detailTemplates_Density[i], destDensity);
            }

            List<string> genericTextureLabels = new List<string>();
            List<RenderTexture> genericTextures = new List<RenderTexture>();
            srcData.GetGenericTextures(genericTextureLabels, genericTextures);
            for (int i = 0; i < genericTextureLabels.Count; ++i)
            {
                RenderTexture srcTexture = genericTextures[i];
                RenderTextureDescriptor desc = srcTexture.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destTexture = new RenderTexture(desc);
                CopyTexture(mat, srcTexture, destTexture);
                destData.AddGenericTexture(genericTextureLabels[i], destTexture);
            }

            RenderTexture srcBiomeMaskMap = srcData.biomeMaskMap;
            if (srcBiomeMaskMap != null)
            {
                RenderTextureDescriptor desc = srcBiomeMaskMap.descriptor;
                desc.width = textureResolution;
                desc.height = textureResolution;
                RenderTexture destBiomeMaskMap = new RenderTexture(desc);
                CopyTexture(mat, srcBiomeMaskMap, destBiomeMaskMap);
                destData.biomeMaskMap = destBiomeMaskMap;
            }

            Rect inBounds = new Rect(srcBounds.min.x, srcBounds.min.z, srcBounds.size.x, srcBounds.size.z);
            Rect outBounds = new Rect(destBounds.min.x, destBounds.min.z, destBounds.size.x, destBounds.size.z);

            List<TreeTemplate> treeTemplates = new List<TreeTemplate>();
            List<ComputeBuffer> srcTreeBuffers = new List<ComputeBuffer>();
            srcData.GetTrees(treeTemplates, srcTreeBuffers);
            for (int i = 0; i < treeTemplates.Count; ++i)
            {
                ComputeBuffer srcBuffer = srcTreeBuffers[i];
                ComputeBuffer destBuffer = BiomeBufferCopy.CopyFrom<InstanceSample>(srcBuffer, inBounds, outBounds);
                if (destBuffer != null)
                {
                    destData.AddTree(treeTemplates[i], destBuffer);
                }
            }

            List<DetailTemplate> detailTemplates_Instance = new List<DetailTemplate>();
            List<ComputeBuffer> detailInstanceBuffers = new List<ComputeBuffer>();
            srcData.GetDetailInstances(detailTemplates_Instance, detailInstanceBuffers);
            for (int i = 0; i < detailTemplates_Instance.Count; ++i)
            {
                ComputeBuffer srcBuffer = detailInstanceBuffers[i];
                ComputeBuffer destBuffer = BiomeBufferCopy.CopyFrom<InstanceSample>(srcBuffer, inBounds, outBounds);
                if (destBuffer != null)
                {
                    destData.AddDetailInstance(detailTemplates_Instance[i], destBuffer);
                }
            }

            List<ObjectTemplate> objectTemplates = new List<ObjectTemplate>();
            List<ComputeBuffer> srcObjectBuffers = new List<ComputeBuffer>();
            srcData.GetObjects(objectTemplates, srcObjectBuffers);
            for (int i = 0; i < objectTemplates.Count; ++i)
            {
                ComputeBuffer srcBuffer = srcObjectBuffers[i];
                ComputeBuffer destBuffer = BiomeBufferCopy.CopyFrom<InstanceSample>(srcBuffer, inBounds, outBounds);
                if (destBuffer != null)
                {
                    destData.AddObject(objectTemplates[i], destBuffer);
                }
            }

            List<string> genericBufferLabels = new List<string>();
            List<ComputeBuffer> srcGenericBuffers = new List<ComputeBuffer>();
            srcData.GetGenericBuffers(genericBufferLabels, srcGenericBuffers);
            for (int i = 0; i < genericBufferLabels.Count; ++i)
            {
                ComputeBuffer srcBuffer = srcGenericBuffers[i];
                ComputeBuffer destBuffer = BiomeBufferCopy.CopyFrom<PositionSample>(srcBuffer, inBounds, outBounds);
                if (destBuffer != null)
                {
                    destData.AddGenericBuffer(genericBufferLabels[i], destBuffer);
                }
            }

            Object.DestroyImmediate(mat);
        }

        private static void CopyTexture(Material mat, RenderTexture src, RenderTexture dest)
        {
            mat.SetTexture(MAIN_TEX, src);
            mat.SetVector(RENDER_TARGET_SIZE, new Vector4(dest.width, dest.height, 0, 0));
            Drawing.DrawQuad(dest, mat, PASS);
        }
    }
}
#endif
