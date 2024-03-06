#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(OffsetNode))]
    public class OffsetNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent MIN_ANGLE = new GUIContent("Min Angle", "The minimum angle indicates offset direction on XZ plane (in degrees)");
        private static readonly GUIContent MAX_ANGLE = new GUIContent("Max Angle", "The maximum angle indicates offset direction on XZ plane (in degrees)");
        private static readonly GUIContent ANGLE_MULTIPLIER = new GUIContent("Angle Multiplier", "A multiplier for direction map");
        private static readonly GUIContent INTENSITY_MULTIPLIER = new GUIContent("Intensity Multiplier", "A multiplier for intensity map");

        public override void OnGUI(INode node)
        {
            OffsetNode n = node as OffsetNode;
            EditorGUI.BeginChangeCheck();
            float minAngle = EditorGUILayout.Slider(MIN_ANGLE, n.minAngle, -360f, 360f);
            float maxAngle = EditorGUILayout.Slider(MAX_ANGLE, n.maxAngle, -360f, 360f);
            float angleMultiplier = EditorGUILayout.Slider(ANGLE_MULTIPLIER, n.angleMultiplier, 0f, 1f);
            float intensityMultiplier = EditorGUILayout.Slider(INTENSITY_MULTIPLIER, n.intensityMultiplier, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.minAngle = minAngle;
                n.maxAngle = maxAngle;
                n.angleMultiplier = angleMultiplier;
                n.intensityMultiplier = intensityMultiplier;
            }
        }
    }
}
#endif
