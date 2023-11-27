#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(TransformPointsNode))]
    public class TransformPointsNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent OFFSET = new GUIContent("Offset", "Offset of the points set in %");
        private static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the points set in degree");
        private static readonly GUIContent SCALE = new GUIContent("Scale", "Scale the points set in %");

        public override void OnGUI(INode node)
        {
            TransformPointsNode n = node as TransformPointsNode;
            EditorGUI.BeginChangeCheck();
            
            Vector2 offset = EditorCommon.InlineVector2Field(OFFSET, n.offset);
            float rotation = EditorGUILayout.FloatField(ROTATION, n.rotation);
            Vector2 scale = EditorCommon.InlineVector2Field(SCALE, n.scale);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);

                n.offset = offset;
                n.rotation = rotation;
                n.scale = scale;
            }
        }
    }
}
#endif
