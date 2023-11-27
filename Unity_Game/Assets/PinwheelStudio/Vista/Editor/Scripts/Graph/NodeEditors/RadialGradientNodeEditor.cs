#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(RadialGradientNode))]
    public class RadialGradientNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent CENTER = new GUIContent("Center", "Center point of the gradient");
        private static readonly GUIContent RADIUS = new GUIContent("Radius", "Radius of the gradient");
        private static readonly GUIContent USE_WORLD_POSITION = new GUIContent("World Position", "Should it use world position or normalized position");
        private static readonly GUIContent REPEAT = new GUIContent("Repeat", "Repeat the pattern");

        public override void OnViewport2dGUI(INode node, Rect imguiRect, Rect imageRect)
        {
            RadialGradientNode n = node as RadialGradientNode;
            if (n.useWorldPosition)
                return;

            PointsDragger.Arguments args = PointsDragger.Arguments.Create();
            Vector2[] points = new Vector2[2];
            points[0] = n.center;
            points[1] = n.center + Vector2.right * n.radius;

            EditorGUI.BeginChangeCheck();
            PointsDragger.DrawCanvas(imguiRect, imageRect, args, points);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                if (n.center == points[0])
                {
                    n.radius = Vector2.Distance(points[0], points[1]);
                }
                else
                {
                    n.center = points[0];
                }
            }
        }

        public override void OnGUI(INode node)
        {
            RadialGradientNode n = node as RadialGradientNode;
            EditorGUI.BeginChangeCheck();
            Vector2 center = EditorCommon.InlineVector2Field(CENTER, n.center);
            float radius = EditorGUILayout.FloatField(RADIUS, n.radius);
            bool useWorldPosition = EditorGUILayout.Toggle(USE_WORLD_POSITION, n.useWorldPosition);
            bool repeat = EditorGUILayout.Toggle(REPEAT, n.repeat);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.center = center;
                n.radius = radius;
                n.useWorldPosition = useWorldPosition;
                n.repeat = repeat;
            }
        }
    }
}
#endif
