#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(FlattenNode))]
    public class FlattenNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent TARGET_HEIGHT = new GUIContent("Target Height", "The target value to flatten to");
        private static readonly GUIContent WORLD_HEIGHT = new GUIContent("World Height", "Is the target height in world space");

        public override void OnGUI(INode node)
        {
            FlattenNode n = node as FlattenNode;
            EditorGUI.BeginChangeCheck();
            float targetHeight = EditorGUILayout.FloatField(TARGET_HEIGHT, n.targetHeight);
            bool useWorldHeight = EditorGUILayout.Toggle(WORLD_HEIGHT, n.useWorldHeight);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.targetHeight = targetHeight;
                n.useWorldHeight = useWorldHeight;
            }
        }
    }
}
#endif
