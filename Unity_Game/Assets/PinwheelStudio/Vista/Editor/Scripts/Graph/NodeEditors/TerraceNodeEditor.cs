#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(TerraceNode))]
    public class TerraceNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent TERRACE_HEADER = new GUIContent("Terrace");
        private static readonly GUIContent STEP_HEIGHT = new GUIContent("Step Height", "Height of a step in world space");
        private static readonly GUIContent REMAP = new GUIContent("Remap", "Remap the step value");

        private static readonly GUIContent HEIGHT_MASK_HEADER = new GUIContent("Height Mask");
        private static readonly GUIContent ENABLE_HEIGHT_MASK = new GUIContent("Enable", "Use the provided height map to mask out the terrace");
        private static readonly GUIContent MIN_HEIGHT = new GUIContent("Min Height", "Minimum height level to apply terrace effect");
        private static readonly GUIContent MAX_HEIGHT = new GUIContent("Max Height", "Maximum height level to apply terrace effect");
        private static readonly GUIContent HEIGHT_BLEND = new GUIContent("Blend", "A curve to blend between original and terraced areas");

        private static readonly GUIContent SLOPE_MASK_HEADER = new GUIContent("Slope Mask");
        private static readonly GUIContent ENABLE_SLOPE_MASK = new GUIContent("Enable", "Use the provided height map to calculate slope steepness and mask out the terrace");
        private static readonly GUIContent MIN_SLOPE = new GUIContent("Min Angle", "Minimum slope angle to apply terrace effect");
        private static readonly GUIContent MAX_SLOPE = new GUIContent("Max Angle", "Maximum slope angle to apply terrace effect");
        private static readonly GUIContent SLOPE_BLEND = new GUIContent("Blend", "A curve to blend between original and terraced areas");

        private static readonly GUIContent OUTLINE_HEADER = new GUIContent("Outline");
        private static readonly GUIContent OUTLINE_POSITION = new GUIContent("Position");
        private static readonly GUIContent OUTLINE_TOLERANCE = new GUIContent("Tolerance");

        public override void OnGUI(INode node)
        {
            TerraceNode n = node as TerraceNode;
            float stepHeight;
            AnimationCurve remap = new AnimationCurve(n.remapCurve.keys);

            bool useHeightMask;
            float minHeight = n.minHeight;
            float maxHeight = n.maxHeight;
            AnimationCurve heightBlend = new AnimationCurve(n.heightBlend.keys);
            bool useSlopeMask;
            float minSlope = n.minSlope;
            float maxSlope = n.maxSlope;
            AnimationCurve slopeBlend = new AnimationCurve(n.slopeBlend.keys);

            EditorGUI.BeginChangeCheck();
            EditorCommon.Header(TERRACE_HEADER);
            stepHeight = EditorGUILayout.FloatField(STEP_HEIGHT, n.stepHeight);
            remap = EditorGUILayout.CurveField(REMAP, n.remapCurve, Color.red, new Rect(0, 0, 1, 1));

            EditorCommon.Header(HEIGHT_MASK_HEADER);
            useHeightMask = EditorGUILayout.Toggle(ENABLE_HEIGHT_MASK, n.useHeightMask);
            if (useHeightMask)
            {
                minHeight = EditorGUILayout.FloatField(MIN_HEIGHT, n.minHeight);
                maxHeight = EditorGUILayout.FloatField(MAX_HEIGHT, n.maxHeight);
                heightBlend = EditorGUILayout.CurveField(HEIGHT_BLEND, n.heightBlend, Color.red, new Rect(0, 0, 1, 1));
            }

            EditorCommon.Header(SLOPE_MASK_HEADER);
            useSlopeMask = EditorGUILayout.Toggle(ENABLE_SLOPE_MASK, n.useSlopeMask);
            if (useSlopeMask)
            {
                minSlope = EditorGUILayout.FloatField(MIN_SLOPE, n.minSlope);
                maxSlope = EditorGUILayout.FloatField(MAX_SLOPE, n.maxSlope);
                slopeBlend = EditorGUILayout.CurveField(SLOPE_BLEND, n.slopeBlend, Color.red, new Rect(0, 0, 1, 1));
            }

            EditorCommon.Header(OUTLINE_HEADER);
            TerraceNode.OutlinePosition outlinePosition = (TerraceNode.OutlinePosition)EditorGUILayout.EnumPopup(OUTLINE_POSITION, n.outlinePosition);
            float outlineTolerance = EditorGUILayout.Slider(OUTLINE_TOLERANCE, n.outlineTolerance, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.stepHeight = stepHeight;
                n.remapCurve = remap;

                n.useHeightMask = useHeightMask;
                n.minHeight = minHeight;
                n.maxHeight = maxHeight;
                n.heightBlend = heightBlend;

                n.useSlopeMask = useSlopeMask;
                n.minSlope = minSlope;
                n.maxSlope = maxSlope;
                n.slopeBlend = slopeBlend;

                n.outlinePosition = outlinePosition;
                n.outlineTolerance = outlineTolerance;
            }

        }
    }
}
#endif
