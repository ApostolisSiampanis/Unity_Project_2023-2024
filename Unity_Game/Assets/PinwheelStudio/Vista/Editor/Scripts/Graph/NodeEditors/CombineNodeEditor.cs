#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(CombineNode))]
    public class CombineNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent MODE = new GUIContent("Mode", "The combine mode");
        private static readonly GUIContent BACKGROUND_MULTIPLIER = new GUIContent("Background Multiplier", "Tune the background before combining");
        private static readonly GUIContent FOREGROUND_MULTIPLIER = new GUIContent("Foreground Multiplier", "Tune the foreground before combining");
        private static readonly GUIContent MASK_MULTIPLIER = new GUIContent("Mask Multiplier", "Tune the mask before combining");

        public override void OnGUI(INode node)
        {
            CombineNode n = node as CombineNode;
            CombineNode.Mode mode;
            float bgMul;
            float fgMul;
            float mMul;
            EditorGUI.BeginChangeCheck();
            mode = (CombineNode.Mode)EditorGUILayout.EnumPopup(MODE, n.mode);
            bgMul = EditorGUILayout.Slider(BACKGROUND_MULTIPLIER, n.backgroundMultiplier, 0f, 2f);
            fgMul = EditorGUILayout.Slider(FOREGROUND_MULTIPLIER, n.foregroundMultiplier, 0f, 2f);
            mMul = EditorGUILayout.Slider(MASK_MULTIPLIER, n.maskMultiplier, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.mode = mode;
                n.backgroundMultiplier = bgMul;
                n.foregroundMultiplier = fgMul;
                n.maskMultiplier = mMul;
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            CombineNode n = node as CombineNode;
            nv.title = $"Combine ({n.mode.ToString()})";
        }
    }
}
#endif
