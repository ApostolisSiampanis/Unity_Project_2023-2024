#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SpreadNode))]
    public class SpreadNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent COUNT = new GUIContent("Count", "Number of new elements to spread from the source element");
        private static readonly GUIContent DISTANCE = new GUIContent("Distance", "Distance of the new elements from the source element");
        private static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");
        private static readonly GUIContent KEEP_SOURCE_POINTS = new GUIContent("Keep Source Points", "Should it include source points in the result?");

        public override void OnGUI(INode node)
        {
            SpreadNode n = node as SpreadNode;
            EditorGUI.BeginChangeCheck();
            int count = EditorGUILayout.IntSlider(COUNT, n.count, 0, 15);
            float distance = EditorGUILayout.Slider(DISTANCE, n.distance, 0f, 1f);
            int seed = EditorGUILayout.IntField(SEED, n.seed);
            bool keepSourcePoints = EditorGUILayout.Toggle(KEEP_SOURCE_POINTS, n.keepSourcePoints);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.count = count;
                n.distance = distance;
                n.seed = seed;
                n.keepSourcePoints = keepSourcePoints;
            }
        }
    }
}
#endif
