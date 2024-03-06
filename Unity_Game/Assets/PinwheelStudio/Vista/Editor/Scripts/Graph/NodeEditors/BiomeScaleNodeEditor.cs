#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(BiomeScaleNode))]
    public class BiomeScaleNodeEditor : ImageNodeEditorBase
    {
        public override void OnGUI(INode node)
        {
            BiomeScaleNode n = node as BiomeScaleNode;
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
            }
        }
    }
}
#endif
