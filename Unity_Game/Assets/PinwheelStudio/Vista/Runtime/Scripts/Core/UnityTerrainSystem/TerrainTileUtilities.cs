#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.UnityTerrain
{
    public class TerrainTileUtilities
    {
        private static readonly string UNITY_HEIGHT_MAP_OUTPUT_SHADER_NAME = "Hidden/Vista/UnityHeightMapOutput";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MAIN_TEX_TEXEL_SIZE = Shader.PropertyToID("_MainTex_TexelSize");
        private static readonly int PASS_HM_TO_UNITY = 0;
        private static readonly int PASS_COLLECT_SCENE_HEIGHT = 1;

        public static void ConvertHeightMapToUnity(RenderTexture src, RenderTexture output)
        {
            Material mat = new Material(ShaderUtilities.Find(UNITY_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, src);
            Drawing.DrawQuad(output, mat, PASS_HM_TO_UNITY);
            Object.DestroyImmediate(mat);
        }

        public static void DecodeAndDrawHeightMap(RenderTexture targetRt, Texture heightMap, Vector2[] quads)
        {
            Material mat = new Material(ShaderUtilities.Find(UNITY_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, heightMap);
            mat.SetVector(MAIN_TEX_TEXEL_SIZE, new Vector4(heightMap.texelSize.x, heightMap.texelSize.y, heightMap.width, heightMap.height));
            Drawing.DrawQuad(targetRt, quads, mat, PASS_COLLECT_SCENE_HEIGHT);
            Object.DestroyImmediate(mat);
        }
    }
}
#endif
