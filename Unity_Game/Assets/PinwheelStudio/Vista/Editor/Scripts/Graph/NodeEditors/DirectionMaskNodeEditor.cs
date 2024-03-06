#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(DirectionMaskNode))]
    public class DirectionMaskNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent ANGLE = new GUIContent("Angle", "Target angle around Y-axis, in degree");
        private static readonly GUIContent TOLERANCE = new GUIContent("Tolerance", "Expand the target angle");
        private static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Remap the mask");

        public override void OnGUI(INode node)
        {
            DirectionMaskNode n = node as DirectionMaskNode;
            EditorGUI.BeginChangeCheck();
            float angle = EditorGUILayout.Slider(ANGLE, n.angle, 0f, 360f);
            float tolerance = EditorGUILayout.Slider(TOLERANCE, n.tolerance, 0f, 180f);
            AnimationCurve falloff = EditorGUILayout.CurveField(FALLOFF, n.falloff, Color.red, new Rect(0, 0, 1, 1));
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.angle = angle;
                n.tolerance = tolerance;
                n.falloff = falloff;
            }
        }
    }
}
#endif
