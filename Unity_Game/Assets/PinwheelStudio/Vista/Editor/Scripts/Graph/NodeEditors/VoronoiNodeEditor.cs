#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(VoronoiNode))]
    public class VoronoiNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent PATTERN_HEADER = new GUIContent("Pattern");
        private static readonly GUIContent OFFSET = new GUIContent("Offset", "Offset the pattern origin");
        private static readonly GUIContent SCALE = new GUIContent("Scale", "Scale of the pattern");
        private static readonly GUIContent AMPLITUDE_EXP = new GUIContent("Amplitude Exponent", "Decide the shape of the pattern, sharp or round, blob like");
        private static readonly GUIContent LACUNARITY = new GUIContent("Lacunarity", "The change in scale of the noise at each layer");
        private static readonly GUIContent PERSISTENCE = new GUIContent("Persistence", "The change in amplitude of the noise at each layer");
        private static readonly GUIContent LAYER_COUNT = new GUIContent("Layer Count", "Number of layer to generate and stack up");
        private static readonly GUIContent INVERSE = new GUIContent("Inverse", "Inverse the pattern to create a blob look");
        private static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");
        private static readonly GUIContent OUTPUT_RAW_CELL = new GUIContent("Output Raw Cell", "Output the pattern as solid cells");

        private static readonly GUIContent WARP_HEADER = new GUIContent("Warp");
        private static readonly GUIContent WARP_MODE = new GUIContent("Mode", "Add warp effect to the noise pattern");
        private static readonly GUIContent WARP_ANGLE_MIN = new GUIContent("Min Angle", "Min rotation of the warp vector");
        private static readonly GUIContent WARP_ANGLE_MAX = new GUIContent("Max Angle", "Max rotation of the warp vector");
        private static readonly GUIContent WARP_INTENSITY = new GUIContent("Intensity", "Strength of the warp effect");

        private static readonly GUIContent REMAP_HEADER = new GUIContent("Remap");
        private static readonly GUIContent REMAP_CURVE = new GUIContent("Remap", "Remap the final noise value");

        public override void OnGUI(INode node)
        {
            VoronoiNode n = node as VoronoiNode;
            Vector2 offset;
            float scale;
            float lacunarity;
            float persistence;
            float amplitudeExp;
            int layerCount;
            bool inverse;
            int seed;

            VoronoiNode.WarpMode warpMode = n.warpMode;
            float warpAngleMin = n.warpAngleMin;
            float warpAngleMax = n.warpAngleMax;
            float warpIntensity = n.warpIntensity;

            AnimationCurve remapCurve = new AnimationCurve(n.remapCurve.keys);

            EditorGUI.BeginChangeCheck();
            EditorCommon.Header(PATTERN_HEADER);
            offset = EditorCommon.InlineVector2Field(OFFSET, n.offset);
            scale = EditorGUILayout.FloatField(SCALE, n.scale);
            amplitudeExp = EditorGUILayout.Slider(AMPLITUDE_EXP, n.amplitudeExponent, 0.01f, 10f);
            lacunarity = EditorGUILayout.FloatField(LACUNARITY, n.lacunarity);
            persistence = EditorGUILayout.Slider(PERSISTENCE, n.persistence, 0f, 1f);
            layerCount = EditorGUILayout.IntSlider(LAYER_COUNT, n.layerCount, 1, 4);
            inverse = EditorGUILayout.Toggle(INVERSE, n.inverse);
            seed = EditorGUILayout.IntField(SEED, n.seed);

            EditorCommon.Header(WARP_HEADER);
            warpMode = (VoronoiNode.WarpMode)EditorGUILayout.EnumPopup(WARP_MODE, n.warpMode);
            if (warpMode == VoronoiNode.WarpMode.Angular)
            {
                warpAngleMin = EditorGUILayout.Slider(WARP_ANGLE_MIN, n.warpAngleMin, -360, 360);
                warpAngleMax = EditorGUILayout.Slider(WARP_ANGLE_MAX, n.warpAngleMax, -360, 360);
            }
            if (warpMode == VoronoiNode.WarpMode.Angular ||
                warpMode == VoronoiNode.WarpMode.Directional)
            {
                warpIntensity = EditorGUILayout.FloatField(WARP_INTENSITY, n.warpIntensity);
            }

            EditorCommon.Header(REMAP_HEADER);
            remapCurve = EditorGUILayout.CurveField(REMAP_CURVE, n.remapCurve, Color.red, new Rect(0, 0, 1, 1));

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.offset = offset;
                n.scale = scale;
                n.lacunarity = lacunarity;
                n.persistence = persistence;
                n.amplitudeExponent = amplitudeExp;
                n.layerCount = layerCount;
                n.inverse = inverse;
                n.seed = seed;

                n.warpMode = warpMode;
                n.warpAngleMin = warpAngleMin;
                n.warpAngleMax = warpAngleMax;
                n.warpIntensity = warpIntensity;

                n.remapCurve = remapCurve;
            }
        }
    }
}
#endif
