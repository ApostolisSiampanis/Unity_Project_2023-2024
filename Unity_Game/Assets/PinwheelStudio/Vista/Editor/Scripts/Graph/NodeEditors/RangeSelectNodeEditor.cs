#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(RangeSelectNode))]
    public class RangeSelectNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent MIN = new GUIContent("Min", "Min value of the range");
        private static GUIContent MAX = new GUIContent("Max", "Max value of the range");

        public override void OnGUI(INode node)
        {
            RangeSelectNode n = node as RangeSelectNode;
            float min;
            float max;
            EditorGUI.BeginChangeCheck();
            min = EditorGUILayout.Slider(MIN, n.min, 0f, 1f);
            max = EditorGUILayout.Slider(MAX, n.max, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.min = min;
                n.max = max;
            }
        }
    }
}
#endif
