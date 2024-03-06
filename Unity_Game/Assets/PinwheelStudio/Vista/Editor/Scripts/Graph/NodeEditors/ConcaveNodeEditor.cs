#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ConcaveNode))]
    public class ConcaveNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent EPSILON = new GUIContent("Epsilon", "Slightly expand/shrink the result");
        private static readonly GUIContent TOLERANCE = new GUIContent("Tolerance", "Relax the comparison to expand the concave area");

        public override void OnGUI(INode node)
        {
            ConcaveNode n = node as ConcaveNode;
            int tolerance;
            EditorGUI.BeginChangeCheck();
            float epsilon = EditorGUILayout.Slider(EPSILON, n.epsilon, -1f, 1f);
            tolerance = EditorGUILayout.IntSlider(TOLERANCE, n.tolerance, 0, 7);
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
