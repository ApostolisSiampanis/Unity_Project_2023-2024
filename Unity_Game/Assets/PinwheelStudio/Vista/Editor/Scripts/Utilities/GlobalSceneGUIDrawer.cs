#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using UnityEditor;

namespace Pinwheel.VistaEditor
{
    [InitializeOnLoad]
    public static class GlobalSceneGUIDrawer
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui += DuringSceneGui;
        }

        private static void DuringSceneGui(SceneView sv)
        {
            DrawBiomeWorkingIcon(sv);
        }

        private static void DrawBiomeWorkingIcon(SceneView sv)
        {
            foreach (LocalProceduralBiome biome in LocalProceduralBiome.allInstances)
            {
                if (biome.isGeneratingCacheData)
                {
                    Handles.Label(biome.transform.position, WaitIconProvider.GetTexture());
                }
            }
        }
    }
}
#endif
