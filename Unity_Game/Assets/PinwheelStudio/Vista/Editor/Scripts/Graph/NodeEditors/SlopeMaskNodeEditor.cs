#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SlopeMaskNode))]
    public class SlopeMaskNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent MIN_ANGLE = new GUIContent("Min Angle", "Minimum surface angle in degree");
        private static readonly GUIContent MAX_ANGLE = new GUIContent("Max Angle", "Maximum surface angle in degree");
        private static readonly GUIContent TRANSITION = new GUIContent("Transition", "A curve to remap the mask");

        public override void OnGUI(INode node)
        {
            SlopeMaskNode n = node as SlopeMaskNode;
            float minAngle;
            float maxAngle;
            AnimationCurve transition;

            EditorGUI.BeginChangeCheck();
            minAngle = EditorGUILayout.Slider(MIN_ANGLE, n.minAngle, 0f, 90f);
            maxAngle = EditorGUILayout.Slider(MAX_ANGLE, n.maxAngle, 0f, 90f);
            transition = EditorGUILayout.CurveField(TRANSITION, n.transition, Color.red, new Rect(0, 0, 1, 1));
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.minAngle = minAngle;
                n.maxAngle = maxAngle;
                n.transition = transition;
            }
        }
    }
}
#endif
