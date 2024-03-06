#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(RotationNode))]
    public class RotationNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent REMAP_01 = new GUIContent("Remap 01", "Remap the radians value to 0-1 range, otherwise it will be in [0, 2PI]");

        public override void OnGUI(INode node)
        {
            RotationNode n = node as RotationNode;
            EditorGUI.BeginChangeCheck();
            bool remap01 = EditorGUILayout.Toggle(REMAP_01, n.remap01);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.remap01 = remap01;
            }
        }
    }
}
#endif
