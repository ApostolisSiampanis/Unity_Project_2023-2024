#if VISTA
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public class AlphaMapsCombiner
    {
        private static readonly string SHADER_NAME = "Hidden/Vista/AlphaMapsCombine";
        private static readonly int LAYER_WEIGHT = Shader.PropertyToID("_LayerWeight");
        private static readonly int CHANNEL_MASK = Shader.PropertyToID("_ChannelMask");

        private static readonly int PASS_COMBINE = 0;
        private static readonly int PASS_COMBINE_MERGE = 1;

        public AlphaMapsCombiner()
        {
        }

        [System.Obsolete]
        public void Combine<T>(List<T> weights, RenderTexture[] alphaMaps) where T : Texture
        {
            int alphaMapCount = (weights.Count + 3) / 4;
            if (alphaMaps.Length != alphaMapCount)
            {
                throw new System.ArgumentException($"Invalid alpha map count. Should be {alphaMapCount}");
            }

            Material mat = new Material(ShaderUtilities.Find(SHADER_NAME));
            for (int i = 0; i < alphaMapCount; ++i)
            {
                for (int channelIndex = 0; channelIndex < 4; ++channelIndex)
                {
                    int weightIndex = i * 4 + channelIndex;
                    Texture t = (weightIndex < weights.Count) ? weights[weightIndex] : (Texture2D.blackTexture as Texture);
                    mat.SetTexture("_Layer" + channelIndex, t);
                }

                Drawing.DrawQuad(alphaMaps[i], mat, PASS_COMBINE);
            }
        }

        public void CombineAndMerge(List<TerrainLayer> srcLayers, List<RenderTexture> srcWeights, int resolution, out List<TerrainLayer> distinctLayers, out List<RenderTexture> alphaMaps)
        {
            distinctLayers = srcLayers.Distinct().ToList();

            int[] layerIndices = new int[srcLayers.Count];
            for (int i = 0; i < srcLayers.Count; ++i)
            {
                TerrainLayer layer = srcLayers[i];
                int index = distinctLayers.IndexOf(layer);
                layerIndices[i] = index;
            }

            int alphaMapCount = (distinctLayers.Count + 3) / 4;
            alphaMaps = new List<RenderTexture>();
            for (int i = 0; i < alphaMapCount; ++i)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                alphaMaps.Add(rt);
            }

            Material mat = new Material(ShaderUtilities.Find(SHADER_NAME));
            for (int i = 0; i < srcWeights.Count; ++i)
            {
                int lIndex = layerIndices[i];
                int tIndex = lIndex / 4;
                int cIndex = lIndex % 4;
                RenderTexture targetRt = alphaMaps[tIndex];
                RenderTexture layerWeight = srcWeights[i];
                Vector4 channelMask = Vector4.zero;
                channelMask[cIndex] = 1;

                mat.SetTexture(LAYER_WEIGHT, layerWeight);
                mat.SetVector(CHANNEL_MASK, channelMask);
                Drawing.DrawQuad(targetRt, mat, PASS_COMBINE_MERGE);
            }
            Object.DestroyImmediate(mat);
        }
    }
}
#endif
