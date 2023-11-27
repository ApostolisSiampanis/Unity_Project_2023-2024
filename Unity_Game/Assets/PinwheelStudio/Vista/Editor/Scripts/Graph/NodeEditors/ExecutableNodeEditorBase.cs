#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public class ExecutableNodeEditorBase : GenericNodeEditor
    {
        private static readonly GUIContent SPLIT_EXECUTION = new GUIContent("Split Execution", "Split the node execution to several frames to reduce hiccup");
        private static readonly GUIContent BYPASS = new GUIContent("Bypass", "Ignore the execution of this node");

        public override void OnBaseGUI(INode node)
        {
            base.OnBaseGUI(node);
            ExecutableNodeBase n = node as ExecutableNodeBase;
            EditorGUI.BeginChangeCheck();
            bool splitExecution = EditorGUILayout.Toggle(SPLIT_EXECUTION, n.shouldSplitExecution);
            bool bypass = n.isBypassed;
            if (!(n is IOutputNode))
            {
                bypass = EditorGUILayout.Toggle(BYPASS, n.isBypassed);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(node);
                n.shouldSplitExecution = splitExecution;
                n.isBypassed = bypass;
            }
        }
    }
}
#endif
