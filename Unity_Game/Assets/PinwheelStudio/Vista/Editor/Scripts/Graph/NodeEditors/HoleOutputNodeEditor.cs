#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(HoleOutputNode))]
    public class HoleOutputNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent THRESHOLD = new GUIContent("Threshold", "Value under this will become 0, otherwise 1");

        public override void OnGUI(INode node)
        {
            HoleOutputNode n = node as HoleOutputNode;
            EditorGUI.BeginChangeCheck();
            float threshold = EditorGUILayout.Slider(THRESHOLD, n.threshold, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.threshold = threshold;
            }

        }
    }
}
#endif
