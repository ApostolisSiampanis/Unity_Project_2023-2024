#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ConstantHeightNode))]
    public class ConstantHeightNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent TARGET_HEIGHT = new GUIContent("Target Height", "Target height value in world space");

        public override void OnGUI(INode node)
        {
            ConstantHeightNode n = node as ConstantHeightNode;
            float targetHeight;
            EditorGUI.BeginChangeCheck();
            targetHeight = EditorGUILayout.FloatField(TARGET_HEIGHT, n.targetHeight);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.targetHeight = targetHeight;
            }
        }
    }
}
#endif
