#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(AngularWarpNode))]
    public class AngularWarpNodeEditor : ImageNodeEditorBase
    {
        private static readonly string UNDO_NAME = "Modify Angular Warp Node";
        private static readonly GUIContent MIN_ANGLE = new GUIContent("Min Angle", "Minimum angle of the warp effect");
        private static readonly GUIContent MAX_ANGLE = new GUIContent("Max Angle", "Maximum angle of the warp effect");
        private static readonly GUIContent DIRECTION_MULTIPLIER = new GUIContent("Direction Multiplier", "Adjust the direction map");
        private static readonly GUIContent INTENSITY_MULTIPLIER = new GUIContent("Intensity Multiplier", "Adjust the intensity map");

        public override void OnGUI(INode node)
        {
            AngularWarpNode n = node as AngularWarpNode;
            EditorGUI.BeginChangeCheck();
            float minAngle = EditorGUILayout.Slider(MIN_ANGLE, n.minAngle, -360, 360);
            float maxAngle = EditorGUILayout.Slider(MAX_ANGLE, n.maxAngle, -360, 360);
            float directionMultiplier = EditorGUILayout.Slider(DIRECTION_MULTIPLIER, n.directionMultiplier, 0, 1);
            float intensityMultiplier = EditorGUILayout.Slider(INTENSITY_MULTIPLIER, n.intensityMultiplier, 0, 1);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(UNDO_NAME);
                n.minAngle = minAngle;
                n.maxAngle = maxAngle;
                n.directionMultiplier = directionMultiplier;
                n.intensityMultiplier = intensityMultiplier;
            }
        }
    }
}
#endif
