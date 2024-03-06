#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using UnityEditor;
using System.IO;

namespace Pinwheel.VistaEditor
{
    [InitializeOnLoad]
    public static class TroubleshootingHandler
    {
        [InitializeOnLoadMethod]
        [RuntimeInitializeOnLoadMethod]
        private static void OnInit()
        {
            TerrainGraph.onBeforeGraphExecution += OnBeforeGraphExecution;
            TerrainGraph.onBeforeNodeExecution += OnBeforeNodeExecution;
            TerrainGraph.onAfterNodeExecution += OnAfterNodeExecution;
        }

        private static void OnBeforeGraphExecution(TerrainGraph graph, TerrainGenerationConfigs data, string[] nodeIds)
        {
            if (EditorSettings.TroubleshootingSettings.IsTroubleshootingModeEnabled())
            {
                string logFilePath = EditorSettings.TroubleshootingSettings.GetExecLogFilePath(graph);
                File.WriteAllText(logFilePath, string.Empty);
            }
        }

        private static void OnBeforeNodeExecution(TerrainGraph graph, TerrainGenerationConfigs data, INode node)
        {
            if (EditorSettings.TroubleshootingSettings.IsTroubleshootingModeEnabled())
            {
                string logFilePath = EditorSettings.TroubleshootingSettings.GetExecLogFilePath(graph);
                File.AppendAllText(logFilePath, $"{node.id}");
            }
        }

        private static void OnAfterNodeExecution(TerrainGraph graph, TerrainGenerationConfigs data, INode node)
        {
            if (EditorSettings.TroubleshootingSettings.IsTroubleshootingModeEnabled())
            {
                string logFilePath = EditorSettings.TroubleshootingSettings.GetExecLogFilePath(graph);
                File.AppendAllText(logFilePath, $" OK\n");
            }
        }
    }
}
#endif
