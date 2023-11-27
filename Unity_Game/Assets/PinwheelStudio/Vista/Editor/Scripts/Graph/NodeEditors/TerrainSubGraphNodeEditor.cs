#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Pinwheel.Vista.Graphics;
using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(TerrainSubGraphNode))]
    public class TerrainSubGraphNodeEditor : ExecutableNodeEditorBase, INeedUpdateNodeVisual
    {
        public static readonly GUIContent GRAPH = new GUIContent("Graph", "The terrain graph asset");
        public static readonly GUIContent EDIT_GRAPH = new GUIContent("Edit");
        public static readonly GUIContent OVERRIDE_SEED = new GUIContent("Override Seed", "If on, it will use its own seed value, otherwise it use the parent graph's seed");
        public static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");

        public void UpdateVisual(INode node, NodeView nv)
        {
            TerrainSubGraphNode n = node as TerrainSubGraphNode;
            if (n.graph == null)
            {
                nv.title = "Sub Graph (not set)";
            }
            else
            {
                nv.title = $"Sub Graph ({n.graph.name})";
            }
        }

        public override void OnGUI(INode node)
        {
            TerrainSubGraphNode n = node as TerrainSubGraphNode;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            TerrainGraph graph = EditorGUILayout.ObjectField(GRAPH, n.graph, typeof(TerrainGraph), false) as TerrainGraph;
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                if (graph != m_graphEditor.sourceGraph)
                {
                    n.graph = graph;
                }
                else
                {
                    n.graph = null;
                }
            }

            GUI.enabled &= n.graph != null;
            if (GUILayout.Button(EDIT_GRAPH, GUILayout.Width(50)))
            {
                if (n.graph != null)
                {
                    TerrainGraph subGraph = n.graph;
                    TerrainGraph parentGraph = m_graphEditor.clonedGraph as TerrainGraph;
                    TerrainGenerationConfigs config = parentGraph.debugConfigs;
                    config.seed = n.seed;
                    subGraph.debugConfigs = config;

                    TerrainSubGraphInputProvider inputProvider = new TerrainSubGraphInputProvider(m_graphEditor as TerrainGraphEditor, n.id);
                    TerrainGraphEditor subgraphEditor = TerrainGraphEditor.OpenGraph<TerrainGraphEditor>(subGraph, inputProvider);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            bool overrideSeed = EditorGUILayout.Toggle(OVERRIDE_SEED, n.overrideSeed);
            int seed = n.seed;
            if (overrideSeed)
            {
                seed = EditorGUILayout.IntField(SEED, n.seed);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);

                n.overrideSeed = overrideSeed;
                n.seed = seed;
            }
        }
    }
}
#endif
