#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(PointGridNode))]
    public class PointGridNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent DENSITY_X = new GUIContent("Density", "Size of the point grid");
        private static readonly GUIContent RELATIVE_TO = new GUIContent("Relative To", "Decide if the density is related to absolute biome size, or number of instance per 100 meters.");

        public override void OnGUI(INode node)
        {
            PointGridNode n = node as PointGridNode;
            EditorGUI.BeginChangeCheck();
            int density = EditorGUILayout.IntField(DENSITY_X, n.density);
            PointGridNode.RelativeTo relativeTo = (PointGridNode.RelativeTo)EditorGUILayout.EnumPopup(RELATIVE_TO, n.relativeTo);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.density = density;
                n.relativeTo = relativeTo;
            }
        }
    }
}
#endif
