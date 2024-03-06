#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(DirectionalWarpNode))]
    public class DirectionalWarpNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent INTENSITY_MULTIPLIER = new GUIContent("Intensity Multiplier", "Adjust the intensity map");

        public override void OnGUI(INode node)
        {
            DirectionalWarpNode n = node as DirectionalWarpNode;
            EditorGUI.BeginChangeCheck();
            float intensityMultiplier = EditorGUILayout.Slider(INTENSITY_MULTIPLIER, n.intensityMultiplier, -1, 1);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.intensityMultiplier = intensityMultiplier;
            }
        }
    }
}
#endif
