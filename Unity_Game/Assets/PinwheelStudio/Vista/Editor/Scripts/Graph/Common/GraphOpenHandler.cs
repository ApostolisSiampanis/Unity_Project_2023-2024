#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public static class GraphOpenHandler
    {
        [OnOpenAsset(0)]
        public static bool HandleOpenGraphAsset(int instanceId, int line)
        {
            Object asset = EditorUtility.InstanceIDToObject(instanceId); 
            if (asset is BiomeMaskGraph bmg)
            {
                BiomeMaskGraphEditor editor = BiomeMaskGraphEditor.OpenGraph<BiomeMaskGraphEditor>(bmg);
                return true;
            }
            else if (asset is TerrainGraph tg)
            {
                TerrainGraphEditor editor = TerrainGraphEditor.OpenGraph<TerrainGraphEditor>(tg);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
#endif
