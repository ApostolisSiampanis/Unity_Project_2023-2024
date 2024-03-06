#if VISTA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Vista.Graph
{
    public static class TerrainGraphUtilities
    {
        public static IEnumerator RequestBiomeData(IBiome biome, BiomeDataRequest request, TerrainGraph graph, Bounds biomeWorldBounds, Space simSpace, int baseResolution, int seed, GraphInputContainer inputContainer = null, BiomeDataMask dataMask = (BiomeDataMask)~0, TerrainGraph.FillArgumentsHandler fillArgumentsCallback = null)
        {
            if (baseResolution < 0 || (baseResolution - 0 * 1) % 8 != 0)
            {
                throw new System.ArgumentException("Invalid base resolution, must be 8*x");
            }

            List<string> nodeIds = new List<string>();
            HeightOutputNode heightOutputNode = null;
            if (dataMask.HasFlag(BiomeDataMask.HeightMap))
            {
                heightOutputNode = graph.GetNode<HeightOutputNode>() as HeightOutputNode;
            }
            if (heightOutputNode != null)
            {
                nodeIds.Add(heightOutputNode.id);
            }

            HoleOutputNode holeOutputNode = null;
            if (dataMask.HasFlag(BiomeDataMask.HoleMap))
            {
                holeOutputNode = graph.GetNode<HoleOutputNode>() as HoleOutputNode;
            }
            if (holeOutputNode != null)
            {
                nodeIds.Add(holeOutputNode.id);
            }

            MeshDensityOutputNode meshDensityOutputNode = null;
            if (dataMask.HasFlag(BiomeDataMask.MeshDensityMap))
            {
                meshDensityOutputNode = graph.GetNode<MeshDensityOutputNode>() as MeshDensityOutputNode;
            }
            if (meshDensityOutputNode != null)
            {
                nodeIds.Add(meshDensityOutputNode.id);
            }

            AlbedoOutputNode albedoOutputNode = null;
            if (dataMask.HasFlag(BiomeDataMask.AlbedoMap))
            {
                albedoOutputNode = graph.GetNode<AlbedoOutputNode>() as AlbedoOutputNode;
            }
            if (albedoOutputNode != null)
            {
                nodeIds.Add(albedoOutputNode.id);
            }

            MetallicSmoothnessOutputNode metallicSmoothnessOutputNode = null;
            if (dataMask.HasFlag(BiomeDataMask.MetallicMap))
            {
                metallicSmoothnessOutputNode = graph.GetNode<MetallicSmoothnessOutputNode>() as MetallicSmoothnessOutputNode;
            }
            if (metallicSmoothnessOutputNode != null)
            {
                nodeIds.Add(metallicSmoothnessOutputNode.id);
            }

            List<TextureOutputNode> textureOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.LayerWeightMaps))
            {
                textureOutputNodes = graph.GetNodesOfType<TextureOutputNode>();
                textureOutputNodes.RemoveAll(n => n.terrainLayer == null);
                textureOutputNodes.Sort((n0, n1) => { return n0.order.CompareTo(n1.order); });
            }
            else
            {
                textureOutputNodes = new List<TextureOutputNode>();
            }
            foreach (TextureOutputNode n in textureOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            List<TreeOutputNode> treeOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.TreeInstances))
            {
                treeOutputNodes = graph.GetNodesOfType<TreeOutputNode>();
                treeOutputNodes.RemoveAll(n => n.treeTemplate == null || !n.treeTemplate.IsValid());
            }
            else
            {
                treeOutputNodes = new List<TreeOutputNode>();
            }
            foreach (TreeOutputNode n in treeOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            List<DetailDensityOutputNode> detailDensityOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.DetailDensityMaps))
            {
                detailDensityOutputNodes = graph.GetNodesOfType<DetailDensityOutputNode>();
                detailDensityOutputNodes.RemoveAll(n => n.detailTemplate == null || !n.detailTemplate.IsValid());
            }
            else
            {
                detailDensityOutputNodes = new List<DetailDensityOutputNode>();
            }
            foreach (DetailDensityOutputNode n in detailDensityOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            List<DetailInstanceOutputNode> detailInstanceOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.DetailInstances))
            {
                detailInstanceOutputNodes = graph.GetNodesOfType<DetailInstanceOutputNode>();
                detailInstanceOutputNodes.RemoveAll(n => n.detailTemplate == null || !n.detailTemplate.IsValid());
            }
            else
            {
                detailInstanceOutputNodes = new List<DetailInstanceOutputNode>();
            }
            foreach (DetailInstanceOutputNode n in detailInstanceOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            List<ObjectOutputNode> objectOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.ObjectInstances))
            {
                objectOutputNodes = graph.GetNodesOfType<ObjectOutputNode>();
                objectOutputNodes.RemoveAll(n => n.objectTemplate == null || !n.objectTemplate.IsValid());
            }
            else
            {
                objectOutputNodes = new List<ObjectOutputNode>();
            }
            foreach (ObjectOutputNode n in objectOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            List<OutputNode> genericOutputNodes = null;
            if (dataMask.HasFlag(BiomeDataMask.GenericTextures) || dataMask.HasFlag(BiomeDataMask.GenericBuffers))
            {
                genericOutputNodes = graph.GetNodesOfType<OutputNode>();
                genericOutputNodes.RemoveAll(n => string.IsNullOrEmpty(n.outputName));
            }
            else
            {
                genericOutputNodes = new List<OutputNode>();
            }
            foreach (OutputNode n in genericOutputNodes)
            {
                nodeIds.Add(n.id);
            }

            if (nodeIds.Count == 0)
            {
                request.Complete();
                yield break;
            }

            Vector2 mpp = new Vector2(biomeWorldBounds.size.x / (baseResolution - 0), biomeWorldBounds.size.z / (baseResolution - 0));
            float rx = simSpace == Space.World ? biomeWorldBounds.min.x : 0;
            float ry = simSpace == Space.World ? biomeWorldBounds.min.z : 0;
            float rw = baseResolution * mpp.x;
            float rh = baseResolution * mpp.y;

            TerrainGenerationConfigs configs = new TerrainGenerationConfigs();
            configs.resolution = baseResolution;
            configs.seed = seed;
            configs.terrainHeight = biomeWorldBounds.size.y;
            configs.worldBounds = new Rect(rx, ry, rw, rh);

#if UNITY_EDITOR
            int editorProgressId = Progress.Start($"Processing biome {biome.gameObject.name}");
#endif
            ExecutionHandle handle = graph.Execute(nodeIds.ToArray(), configs, inputContainer, fillArgumentsCallback);
            while(!handle.isCompleted)
            {
#if UNITY_EDITOR
                Progress.Report(editorProgressId, handle.progress.totalProgress);
#endif
                yield return null;
            }

            if (heightOutputNode != null)
            {
                RenderTexture generatedHeightMap = handle.data.RemoveRTFromPool(heightOutputNode.mainOutputSlot);
                request.data.heightMap = generatedHeightMap;
            }
            yield return null;

            if (holeOutputNode != null)
            {
                RenderTexture generatedHoleMap = handle.data.RemoveRTFromPool(holeOutputNode.mainOutputSlot);
                request.data.holeMap = generatedHoleMap;
            }
            yield return null;

            if (meshDensityOutputNode != null)
            {
                RenderTexture generatedmeshDensityMap = handle.data.RemoveRTFromPool(meshDensityOutputNode.mainOutputSlot);
                request.data.meshDensityMap = generatedmeshDensityMap;
            }
            yield return null;

            if (albedoOutputNode != null)
            {
                RenderTexture generatedAlbedoMap = handle.data.RemoveRTFromPool(albedoOutputNode.mainOutputSlot);
                request.data.albedoMap = generatedAlbedoMap;
            }
            yield return null;

            if (metallicSmoothnessOutputNode != null)
            {
                RenderTexture generatedMetallicMap = handle.data.RemoveRTFromPool(metallicSmoothnessOutputNode.mainOutputSlot);
                request.data.metallicMap = generatedMetallicMap;
            }
            yield return null;

            foreach (TextureOutputNode n in textureOutputNodes)
            {
                RenderTexture generatedWeight = handle.data.RemoveRTFromPool(n.mainOutputSlot);
                request.data.AddTextureLayer(n.terrainLayer, generatedWeight);
            }
            yield return null;

            foreach (TreeOutputNode n in treeOutputNodes)
            {
                GraphBuffer treeBuffer = handle.data.RemoveBufferFromPool(n.mainOutputSlot);
                if (treeBuffer != null && treeBuffer.buffer != null)
                {
                    request.data.AddTree(n.treeTemplate, treeBuffer.buffer);
                }
            }
            yield return null;

            foreach (DetailDensityOutputNode n in detailDensityOutputNodes)
            {
                RenderTexture generatedDensityMap = handle.data.RemoveRTFromPool(n.mainOutputSlot);
                request.data.AddDetailDensity(n.detailTemplate, generatedDensityMap);
            }
            yield return null;

            foreach (DetailInstanceOutputNode n in detailInstanceOutputNodes)
            {
                GraphBuffer instanceBuffer = handle.data.RemoveBufferFromPool(n.mainOutputSlot);
                if (instanceBuffer != null && instanceBuffer.buffer != null)
                {
                    request.data.AddDetailInstance(n.detailTemplate, instanceBuffer.buffer);
                }
            }
            yield return null;

            foreach (ObjectOutputNode n in objectOutputNodes)
            {
                GraphBuffer objectBuffer = handle.data.RemoveBufferFromPool(n.mainOutputSlot);
                if (objectBuffer != null && objectBuffer.buffer != null)
                {
                    request.data.AddObject(n.objectTemplate, objectBuffer.buffer);
                }
            }
            yield return null;

            foreach (OutputNode n in genericOutputNodes)
            {
                if (n.slotType.Equals(typeof(BufferSlot)))
                {
                    if (dataMask.HasFlag(BiomeDataMask.GenericBuffers))
                    {
                        GraphBuffer buffer = handle.data.RemoveBufferFromPool(n.mainOutputSlot);
                        if (buffer != null && buffer.buffer != null)
                        {
                            request.data.AddGenericBuffer(n.outputName, buffer.buffer);
                        }
                    }
                }
                else if (n.slotType.Equals(typeof(MaskSlot)) || n.slotType.Equals(typeof(ColorTextureSlot)))
                {
                    if (dataMask.HasFlag(BiomeDataMask.GenericTextures))
                    {
                        RenderTexture generatedTexture = handle.data.RemoveRTFromPool(n.mainOutputSlot);
                        if (generatedTexture != null)
                        {
                            request.data.AddGenericTexture(n.outputName, generatedTexture);
                        }
                    }
                }
            }

            handle.Dispose();
            request.Complete();

#if UNITY_EDITOR
            Progress.Finish(editorProgressId);
#endif
        }
    }
}
#endif
