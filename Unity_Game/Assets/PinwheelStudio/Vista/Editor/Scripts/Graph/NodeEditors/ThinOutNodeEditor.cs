#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ThinOutNode))]
    public class TrimNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent MASK_MULTIPLIER = new GUIContent("Mask Multiplier", "Multiplier for Mask texture");
        private static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");

        public override void OnGUI(INode node)
        {
            ThinOutNode n = node as ThinOutNode;
            EditorGUI.BeginChangeCheck();
            float maskMultiplier = EditorGUILayout.Slider(MASK_MULTIPLIER, n.maskMultiplier, 0f, 2f);
            int seed = EditorGUILayout.IntField(SEED, n.seed);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.maskMultiplier = maskMultiplier;
                n.seed = seed;
            }
        }

    }
}
#endif
