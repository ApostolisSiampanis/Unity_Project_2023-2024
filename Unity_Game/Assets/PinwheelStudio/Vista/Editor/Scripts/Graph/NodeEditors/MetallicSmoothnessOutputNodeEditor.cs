#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(MetallicSmoothnessOutputNode))]
    public class MetallicSmoothnessOutputNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent METALLIC_MULTIPLIER = new GUIContent("Metallic Multiplier", "Multiplier for metallic value (R channel)");
        private static readonly GUIContent SMOOTHNESS_MULTIPLIER = new GUIContent("Smoothness Multiplier", "Multiplier for smoothness value (A channel)");

        public override void OnGUI(INode node)
        {
            MetallicSmoothnessOutputNode n = node as MetallicSmoothnessOutputNode;
            EditorGUI.BeginChangeCheck();
            float metallicMultiplier = EditorGUILayout.FloatField(METALLIC_MULTIPLIER, n.metallicMultiplier);
            float smoothnessMultiplier = EditorGUILayout.FloatField(SMOOTHNESS_MULTIPLIER, n.smoothnessMultiplier);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.metallicMultiplier = metallicMultiplier;
                n.smoothnessMultiplier = smoothnessMultiplier;
            }
        }
    }
}
#endif
