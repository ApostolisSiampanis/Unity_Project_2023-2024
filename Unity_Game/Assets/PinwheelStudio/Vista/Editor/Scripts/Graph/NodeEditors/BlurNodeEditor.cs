#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(BlurNode))]
    public class BlurNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent RADIUS = new GUIContent("Radius", "Radius of the blur effect");

        public override void OnGUI(INode node)
        {
            BlurNode n = node as BlurNode;
            int radius;
            EditorGUI.BeginChangeCheck();
            radius = EditorGUILayout.IntSlider(RADIUS, n.radius, 0, 100);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.radius = radius;
            }
        }
    }
}
#endif
