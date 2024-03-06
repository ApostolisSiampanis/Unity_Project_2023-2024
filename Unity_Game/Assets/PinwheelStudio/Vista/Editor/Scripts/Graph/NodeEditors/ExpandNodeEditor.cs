#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ExpandNode))]
    public class ExpandNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent RATIO = new GUIContent("Ratio", "Expansion ratio, how fast a pixel bleed its content to neighbor pixels");
        private static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration", "How many iterations to simulate");
        private static readonly GUIContent ITERATION_PER_FRAME = new GUIContent("Iteration Per Frame", "How many iterations to perform in a frame");

        public override void OnGUI(INode node)
        {
            ExpandNode n = node as ExpandNode;
            EditorGUI.BeginChangeCheck();
            float ratio = EditorGUILayout.Slider(RATIO, n.ratio, 0f, 1f);
            int iteration = EditorGUILayout.IntField(ITERATION_COUNT, n.iterationCount);
            int iterationPerFrame = EditorGUILayout.IntField(ITERATION_PER_FRAME, n.iterationPerFrame);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.ratio = ratio;
                n.iterationCount = iteration;
                n.iterationPerFrame = iterationPerFrame;
            }
        }
    }
}
#endif
