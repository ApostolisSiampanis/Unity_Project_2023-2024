#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(FalloffDetailNode))]
    public class FalloffDetailNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent DETAIL_MULTIPLIER = new GUIContent("Detail Multiplier", "The multiplier for the detail mask");

        public override void OnGUI(INode node)
        {
            FalloffDetailNode n = node as FalloffDetailNode;
            EditorGUI.BeginChangeCheck();
            float detailMultiplier = EditorGUILayout.Slider(DETAIL_MULTIPLIER, n.detailMultiplier, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.detailMultiplier = detailMultiplier;
            }
        }
    }
}
#endif
