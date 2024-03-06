#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ThresholdNode))]
    public class ThresholdNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent LOW = new GUIContent("Low", "Lower threshold, value below this point will turn 0");
        private static GUIContent HIGH = new GUIContent("High", "Upper threshold, value above this point will turn 1");
        private static GUIContent COMPARE_MODE = new GUIContent("Compare Mode", "How to compare the value against the threshold");

        public override void OnGUI(INode node)
        {
            ThresholdNode n = node as ThresholdNode;
            float low;
            float high;
            ThresholdNode.CompareMode compareMode;
            EditorGUI.BeginChangeCheck();
            low = EditorGUILayout.Slider(LOW, n.low, 0f, 1f);
            high = EditorGUILayout.Slider(HIGH, n.high, 0f, 1f);
            compareMode = (ThresholdNode.CompareMode)EditorGUILayout.EnumPopup(COMPARE_MODE, n.compareMode);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.low = low;
                n.high = high;
                n.compareMode = compareMode;
            }
        }
    }
}
#endif
