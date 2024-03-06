#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ConvexNode))]
    public class ConvexNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent EPSILON = new GUIContent("Epsilon", "Slightly expand/shrink the result");
        private static readonly GUIContent TOLERANCE = new GUIContent("Tolerance", "Minimum number of neighbor pixels that should pass the convex test");

        public override void OnGUI(INode node)
        {
            ConvexNode n = node as ConvexNode;
            EditorGUI.BeginChangeCheck();
            float epsilon = EditorGUILayout.Slider(EPSILON, n.epsilon, -1f, 1f);
            int tolerance = EditorGUILayout.IntSlider(TOLERANCE, n.tolerance, 0, 7);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.epsilon = epsilon;
                n.tolerance = tolerance;
            }
        }
    }
}
#endif
