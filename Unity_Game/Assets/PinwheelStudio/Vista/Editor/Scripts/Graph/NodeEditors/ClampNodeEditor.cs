#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ClampNode))]
    public class ClampNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent MIN = new GUIContent("Min", "Min value, mask value cannot go below this");
        private static GUIContent MAX = new GUIContent("Max", "Max value, mask value cannot go above this");

        public override void OnGUI(INode node)
        {
            ClampNode n = node as ClampNode;
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
