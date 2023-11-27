#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Pinwheel.Vista;

namespace Pinwheel.VistaEditor.Graph
{
    [CustomEditor(typeof(TerrainGraph))]
    public class TerrainGraphInspector : Editor
    {
        public delegate void InspectorGuiHandler(TerrainGraph target);
        public static event InspectorGuiHandler inspectorGuiCallback;

        private TerrainGraph instance;

        private void OnEnable()
        {
            instance = target as TerrainGraph;
        }

        public override void OnInspectorGUI()
        {
            inspectorGuiCallback?.Invoke(instance);
        }       
    }
}
#endif
