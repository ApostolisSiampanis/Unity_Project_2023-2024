#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(LoadTextureNode))]
    public class LoadTextureNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent TEXTURE = new GUIContent("Texture", "The texture to load");

        public override void OnGUI(INode node)
        {
            LoadTextureNode n = node as LoadTextureNode;
            EditorGUI.BeginChangeCheck();
            Texture tex = EditorGUILayout.ObjectField(TEXTURE, n.texture, typeof(Texture), false) as Texture;
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.texture = tex;
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            LoadTextureNode n = node as LoadTextureNode;
            Texture preview = null;
            if (n.texture != null)
            {
                preview = AssetPreview.GetAssetPreview(n.texture);
            }
            nv.SetPreviewImage(preview);
        }
    }
}
#endif
