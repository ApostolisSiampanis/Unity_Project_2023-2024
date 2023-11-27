#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SharpenNode))]
    public class SharpenNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent INTENSITY = new GUIContent("Intensity", "Strength of the sharpen effect");

        public override void OnGUI(INode node)
        {
            SharpenNode n = node as SharpenNode;
            float intensity;
            EditorGUI.BeginChangeCheck();
            intensity = EditorGUILayout.FloatField(INTENSITY, n.intensity);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.intensity = intensity;
            }
        }
    }
}
#endif
