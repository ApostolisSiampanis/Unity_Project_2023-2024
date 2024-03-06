#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ShapeNode))]
    public class ShapeNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent SHAPE = new GUIContent("Shape", "Shape type to generate");
        private static readonly GUIContent SCALE = new GUIContent("Scale", "Scale the whole shape");
        private static readonly GUIContent INNER_SIZE = new GUIContent("Inner Size", "Size of the inner part of some shape");

        public override void OnGUI(INode node)
        {
            ShapeNode n = node as ShapeNode;
            ShapeNode.Shape shape;
            Vector2 scale;
            float innerSize = n.innerSize;
            EditorGUI.BeginChangeCheck();
            shape = (ShapeNode.Shape)EditorGUILayout.EnumPopup(SHAPE, n.shape);
            scale = EditorCommon.Vector2Slider(SCALE, n.scale, 0f, 1f);
            if (shape == ShapeNode.Shape.Torus ||
                shape == ShapeNode.Shape.Brick)
            {
                innerSize = EditorGUILayout.Slider(INNER_SIZE, n.innerSize, 0f, 1f);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.shape = shape;
                n.scale = scale;
                n.innerSize = innerSize;
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            ShapeNode n = node as ShapeNode;
            nv.title = $"Shape ({n.shape.ToString()})";
        }
    }
}
#endif
