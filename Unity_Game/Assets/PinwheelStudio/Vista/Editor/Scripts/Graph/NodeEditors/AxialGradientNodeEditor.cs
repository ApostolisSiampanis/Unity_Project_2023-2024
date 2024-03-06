#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(AxialGradientNode))]
    public class AxialGradientNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent START_POINT = new GUIContent("Start Point", "The gradient start point");
        private static readonly GUIContent END_POINT = new GUIContent("End Point", "The gradient end point");
        private static readonly GUIContent USE_WORLD_POSITION = new GUIContent("World Position", "Should it use world position or normalized position");
        private static readonly GUIContent REPEAT = new GUIContent("Repeat", "Should the gradient repeated or clamped");

        public override void OnViewport2dGUI(INode node, Rect imguiRect, Rect imageRect)
        {
            AxialGradientNode n = node as AxialGradientNode;
            if (n.useWorldPosition)
                return;

            PointsDragger.Arguments args = PointsDragger.Arguments.Create();

            Vector2[] points = new Vector2[] { n.startPoint, n.endPoint };
            EditorGUI.BeginChangeCheck();
            PointsDragger.DrawCanvas(imguiRect, imageRect, args, points);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.startPoint = points[0];
                n.endPoint = points[1];
            }
        }

        public override void OnGUI(INode node)
        {
            AxialGradientNode n = node as AxialGradientNode;
            EditorGUI.BeginChangeCheck();
            Vector2 startPoint = EditorCommon.InlineVector2Field(START_POINT, n.startPoint);
            Vector2 endPoint = EditorCommon.InlineVector2Field(END_POINT, n.endPoint);
            bool useWorldPos = EditorGUILayout.Toggle(USE_WORLD_POSITION, n.useWorldPosition);
            bool repeat = EditorGUILayout.Toggle(REPEAT, n.repeat);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.startPoint = startPoint;
                n.endPoint = endPoint;
                n.useWorldPosition = useWorldPos;
                n.repeat = repeat;
            }
        }
    }
}
#endif
