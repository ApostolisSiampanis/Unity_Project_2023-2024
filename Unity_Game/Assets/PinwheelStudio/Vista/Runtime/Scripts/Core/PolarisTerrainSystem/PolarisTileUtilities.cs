#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.PolarisTerrain
{
    public class PolarisTileUtilities
    {
        private static readonly string POLARIS_HEIGHT_MAP_OUTPUT_SHADER_NAME = "Hidden/Vista/PolarisHeightMapOutput";
        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int NEW_HEIGHT_DATA = Shader.PropertyToID("_NewHeightData");
        private static readonly int NEW_HOLE_DATA = Shader.PropertyToID("_NewHoleData");
        private static readonly int NEW_DENSITY_DATA = Shader.PropertyToID("_NewDensityData");
        private static readonly int PASS_HEIGHT = 0;
        private static readonly int PASS_HOLE = 1;
        private static readonly int PASS_MESH_DENSITY = 2;
        private static readonly int PASS_COLLECT_SCENE_HEIGHT = 3;

        public static void SetHeightMap(Texture currentHeightMap, RenderTexture srcHeightData, RenderTexture destHeightMap)
        {
            Material mat = new Material(ShaderUtilities.Find(POLARIS_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(HEIGHT_MAP, currentHeightMap);
            mat.SetTexture(NEW_HEIGHT_DATA, srcHeightData);
            Drawing.DrawQuad(destHeightMap, mat, PASS_HEIGHT);
            Object.DestroyImmediate(mat);
        }

        public static void SetHoleMap(Texture currentHeightMap, RenderTexture srcHoleData, RenderTexture destHeightMap)
        {
            Material mat = new Material(ShaderUtilities.Find(POLARIS_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(HEIGHT_MAP, currentHeightMap);
            mat.SetTexture(NEW_HOLE_DATA, srcHoleData);
            Drawing.DrawQuad(destHeightMap, mat, PASS_HOLE);
            Object.DestroyImmediate(mat);
        }

        public static void SetMeshDensityMap(Texture currentHeightMap, RenderTexture srcDensityData, RenderTexture destHeightMap)
        {
            Material mat = new Material(ShaderUtilities.Find(POLARIS_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(HEIGHT_MAP, currentHeightMap);
            mat.SetTexture(NEW_DENSITY_DATA, srcDensityData);
            Drawing.DrawQuad(destHeightMap, mat, PASS_MESH_DENSITY);
            Object.DestroyImmediate(mat);
        }

        public static void DecodeAndDrawHeightMap(RenderTexture targetRt, Texture heightMap, Vector2[] quads)
        {
            Material mat = new Material(ShaderUtilities.Find(POLARIS_HEIGHT_MAP_OUTPUT_SHADER_NAME));
            mat.SetTexture(HEIGHT_MAP, heightMap);
            Drawing.DrawQuad(targetRt, quads, mat, PASS_COLLECT_SCENE_HEIGHT);
            Object.DestroyImmediate(mat);
        }
    }
}
#endif
