#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ContrastBrightnessNode))]
    public class ContrastBrightnessNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent CONTRAST = new GUIContent("Contrast", "Adjusting image contrast, low contrast will flatten out the terrain, where high contrast makes terrain features more distinctive");
        private static GUIContent BRIGHTNESS = new GUIContent("Brightness", "Adjusting image intensity, or the overall high of the terrain");

        public override void OnGUI(INode node)
        {
            ContrastBrightnessNode n = node as ContrastBrightnessNode;
            float contrast;
            float brightness;
            EditorGUI.BeginChangeCheck();
            contrast = EditorGUILayout.Slider(CONTRAST, n.contrast, -1f, 1f);
            brightness = EditorGUILayout.Slider(BRIGHTNESS, n.brightness, -1f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.contrast = contrast;
                n.brightness = brightness;
            }
        }
    }
}
#endif
