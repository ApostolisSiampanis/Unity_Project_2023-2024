#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(LevelsNode))]
    public class LevelsNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent IN_LOW = new GUIContent("In Low", "Make the dark areas of the image darker, can be used to adjust the low-altitude areas of the terrain");
        private static GUIContent IN_MID = new GUIContent("In Mid", "Adjust the value range mid-point");
        private static GUIContent IN_HIGH = new GUIContent("In High", "Make the bright areas of the image brighter, can be used to adjust the high-altitude areas of the terrain");
        private static GUIContent OUT_LOW = new GUIContent("Out Low", "Lower bound to remap the value range");
        private static GUIContent OUT_HIGH = new GUIContent("Out High", "Upper bound to remap the value range");


        public override void OnGUI(INode node)
        {
            LevelsNode n = node as LevelsNode;
            float inLow;
            float inMid;
            float inHigh;
            float outLow;
            float outHigh;
            EditorGUI.BeginChangeCheck();
            inLow = EditorGUILayout.Slider(IN_LOW, n.inLow, 0f, 1f);
            inMid = EditorGUILayout.Slider(IN_MID, n.inMid, 0f, 1f);
            inHigh = EditorGUILayout.Slider(IN_HIGH, n.inHigh, 0f, 1f);
            outLow = EditorGUILayout.Slider(OUT_LOW, n.outLow, 0f, 1f);
            outHigh = EditorGUILayout.Slider(OUT_HIGH, n.outHigh, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.inLow = inLow;
                n.inMid = inMid;
                n.inHigh = inHigh;
                n.outLow = outLow;
                n.outHigh = outHigh;
            }
        }
    }
}
#endif
