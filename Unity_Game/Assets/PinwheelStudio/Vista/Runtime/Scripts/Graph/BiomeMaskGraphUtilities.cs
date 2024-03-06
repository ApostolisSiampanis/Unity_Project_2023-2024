#if VISTA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public static class BiomeMaskGraphUtilities
    {
        public static IEnumerator RequestData(BiomeDataRequest request, BiomeMaskGraph graph, Bounds worldBounds, Space simSpace, RenderTexture baseBiomeMask)
        {
            int baseResolution = baseBiomeMask.width;
            if (baseResolution < 0 || (baseResolution - 0 * 1) % 8 != 0)
            {
                throw new System.ArgumentException("Invalid base resolution, must be 8*x");
            }

            List<OutputNode> genericOutputNodes = graph.GetNodesOfType<OutputNode>();
            OutputNode targetOutputNode = genericOutputNodes.Find(n => GraphConstants.BIOME_MASK_OUTPUT_NAME.Equals(n.outputName));
            if (targetOutputNode == null)
            {
                request.Complete();
                yield break;
            }

            string[] nodeIds = new string[] { targetOutputNode.id };

            TerrainGenerationConfigs configs = new TerrainGenerationConfigs();
            configs.resolution = baseResolution;
            configs.seed = 0;
            configs.terrainHeight = worldBounds.size.y;
            configs.worldBounds = new Rect(simSpace == Space.World ? worldBounds.min.x : 0, simSpace == Space.World ? worldBounds.min.z : 0, worldBounds.size.x, worldBounds.size.z);

            GraphInputContainer inputContainer = new GraphInputContainer();
            inputContainer.AddTexture(GraphConstants.BIOME_MASK_INPUT_NAME, baseBiomeMask);

            ExecutionHandle handle = graph.Execute(nodeIds, configs, inputContainer);
            yield return handle;

            RenderTexture rt = handle.data.RemoveRTFromPool(new SlotRef(targetOutputNode.id, targetOutputNode.mainOutputSlot.slotId));
            if (rt != null)
            {
                request.data.biomeMaskMap = rt;
            }

            handle.Dispose();
            request.Complete();
        }
    }
}
#endif
