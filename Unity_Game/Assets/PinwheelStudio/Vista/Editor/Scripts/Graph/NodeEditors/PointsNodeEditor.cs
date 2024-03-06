#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.VistaEditor;
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(PointsNode))]
    public class PointsNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent SPACING = new GUIContent("Spacing", "Distance between points in world meters");

        public override void OnGUI(INode node)
        {
            PointsNode n = node as PointsNode;
            EditorGUI.BeginChangeCheck();
            Vector2 spacing = EditorCommon.InlineVector2Field(SPACING, n.spacing);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.spacing = spacing;
            }
        }
    }
}
#endif
