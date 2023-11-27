#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SmoothNode))]
    public class SmoothNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration Count", "Number of smooth iteration");
        private static readonly GUIContent ITERATION_PER_FRAME = new GUIContent("Iteration Per Frame", "Number of iteration to perform in a frame if Split Execution is on");

        public override void OnGUI(INode node)
        {
            SmoothNode n = node as SmoothNode;
            int iterationCount;
            int iterationPerFrame;
            EditorGUI.BeginChangeCheck();
            iterationCount = EditorGUILayout.IntField(ITERATION_COUNT, n.iterationCount);
            iterationPerFrame = EditorGUILayout.IntField(ITERATION_PER_FRAME, n.iterationPerFrame);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.iterationCount = iterationCount;
                n.iterationPerFrame = iterationPerFrame;
            }
        }
    }
}
#endif
