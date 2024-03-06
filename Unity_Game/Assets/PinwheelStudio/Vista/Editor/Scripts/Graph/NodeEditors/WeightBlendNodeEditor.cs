#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(WeightBlendNode))]
    public class WeightBlendNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent LAYER_COUNT = new GUIContent("Layer Count", "Number of layer to blend. Layer with higher index will be 'painted' on top of the lower ones. Similar to how you use the Paint Texture in Terrain Tool");

        public override void OnGUI(INode node)
        {
            WeightBlendNode n = node as WeightBlendNode;
            EditorGUI.BeginChangeCheck();
            int layerCount = EditorGUILayout.DelayedIntField(LAYER_COUNT, n.layerCount);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.layerCount = layerCount;
            }
        }
    }
}
#endif
