#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ColorBlendNode))]
    public class ColorBlendNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent LAYER_COUNT = new GUIContent("Layer Count", "Number of layer to paint. Layer with higher index will be painted on top of the lower ones");

        public override void OnGUI(INode node)
        {
            ColorBlendNode n = node as ColorBlendNode;

            EditorGUI.BeginChangeCheck();
            int layerCount = EditorGUILayout.DelayedIntField(LAYER_COUNT, n.layerCount);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.layerCount = layerCount;
            }

            for (int i = 0; i < layerCount; ++i)
            {
                EditorGUI.BeginChangeCheck();
                Color color = EditorGUILayout.ColorField($"Color {i}", n.layerColors[i]);
                if (EditorGUI.EndChangeCheck())
                {
                    m_graphEditor.RegisterUndo(n);
                    n.layerColors[i] = color;
                }
            }
        }
    }
}
#endif
