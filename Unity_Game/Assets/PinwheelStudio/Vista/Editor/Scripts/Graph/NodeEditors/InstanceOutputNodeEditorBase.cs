#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public abstract class InstanceOutputNodeEditorBase : ExecutableNodeEditorBase
    {
        private static readonly GUIContent DENSITY_MULTIPLIER = new GUIContent("Density Multiplier", "Adjust the density map");
        private static readonly GUIContent VERTICAL_SCALE_MULTIPLIER = new GUIContent("Vertical Scale Multiplier", "Adjust the height scale map");
        private static readonly GUIContent HORIZONTAL_SCALE_MULTIPLIER = new GUIContent("Horizontal Scale Multiplier", "Adjust the width scale map");
        private static readonly GUIContent MIN_ROTATION = new GUIContent("Min Rotation", "Min tree rotation in degree");
        private static readonly GUIContent MAX_ROTATION = new GUIContent("Max Rotation", "Max tree rotation in degree");
        private static readonly GUIContent ROTATION_MAP_MULTIPLIER = new GUIContent("Rotation Multiplier", "Adjust the rotation map");
        private static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");

        public override void OnGUI(INode node)
        {
            InstanceOutputNodeBase n = node as InstanceOutputNodeBase;
            EditorGUI.BeginChangeCheck();
            float densityMultiplier = EditorGUILayout.FloatField(DENSITY_MULTIPLIER, n.densityMultiplier);
            float heightScaleMultiplier = EditorGUILayout.FloatField(VERTICAL_SCALE_MULTIPLIER, n.verticalScaleMultiplier);
            float widthScaleMultiplier = EditorGUILayout.FloatField(HORIZONTAL_SCALE_MULTIPLIER, n.horizontalScaleMultiplier);
            float minRotation = EditorGUILayout.Slider(MIN_ROTATION, n.minRotation, -360f, 360f);
            float maxRotation = EditorGUILayout.Slider(MAX_ROTATION, n.maxRotation, -360f, 360f);
            float rotationMultiplier = EditorGUILayout.FloatField(ROTATION_MAP_MULTIPLIER, n.rotationMultiplier);
            int seed = EditorGUILayout.IntField(SEED, n.seed);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.densityMultiplier = densityMultiplier;
                n.verticalScaleMultiplier = heightScaleMultiplier;
                n.horizontalScaleMultiplier = widthScaleMultiplier;
                n.minRotation = minRotation;
                n.maxRotation = maxRotation;
                n.rotationMultiplier = rotationMultiplier;
                n.seed = seed;
            }
        }
    }
}
#endif
