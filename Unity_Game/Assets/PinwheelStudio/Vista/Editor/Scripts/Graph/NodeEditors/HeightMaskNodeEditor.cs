#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(HeightMaskNode))]
    public class HeightMaskNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent MIN_HEIGHT = new GUIContent("Min Height", "Minimum height level");
        private static readonly GUIContent MAX_HEIGHT = new GUIContent("Max Height", "Maximum height level");
        private static readonly GUIContent TRANSITION = new GUIContent("Transition", "A curve to remap the mask");

        public override void OnGUI(INode node)
        {
            HeightMaskNode n = node as HeightMaskNode;
            float minHeight;
            float maxHeight;
            AnimationCurve transition;

            EditorGUI.BeginChangeCheck();
            minHeight = EditorGUILayout.FloatField(MIN_HEIGHT, n.minHeight);
            maxHeight = EditorGUILayout.FloatField(MAX_HEIGHT, n.maxHeight);
            transition = EditorGUILayout.CurveField(TRANSITION, n.transition, Color.red, new Rect(0, 0, 1, 1));
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.minHeight = minHeight;
                n.maxHeight = maxHeight;
                n.transition = transition;
            }
        }
    }
}
#endif
