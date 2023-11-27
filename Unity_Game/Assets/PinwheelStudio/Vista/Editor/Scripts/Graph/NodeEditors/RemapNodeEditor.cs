#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(RemapNode))]
    public class RemapNodeEditor : ImageNodeEditorBase
    {
        public enum RangeMode
        {
            Unit, Any
        }

        private const string PREF_RANGE_MODE = "vista-remap-node-range-mode";

        private static readonly GUIContent RANGE = new GUIContent("Range", "The min/max remap range");
        private static readonly GUIContent MIN = new GUIContent("Min", "Output range min value");
        private static readonly GUIContent MAX = new GUIContent("Max", "Output range max value");

        public override void OnGUI(INode node)
        {
            RemapNode n = node as RemapNode;
            float min = n.min;
            float max = n.max;
            EditorGUI.BeginChangeCheck();
            RangeMode mode = (RangeMode)EditorPrefs.GetInt(PREF_RANGE_MODE, (int)RangeMode.Unit);
            mode = (RangeMode)EditorGUILayout.EnumPopup(RANGE, mode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(PREF_RANGE_MODE, (int)mode);
            }

            EditorGUI.BeginChangeCheck();
            if (mode == RangeMode.Unit)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");
                min = EditorGUILayout.FloatField(min, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 1f, GUILayout.ExpandWidth(true));
                max = EditorGUILayout.FloatField(max, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
            }
            else if (mode == RangeMode.Any)
            {
                EditorGUI.indentLevel += 1;
                min = EditorGUILayout.FloatField(MIN, n.min);
                max = EditorGUILayout.FloatField(MAX, n.max);
                EditorGUI.indentLevel -= 1;
            }
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
