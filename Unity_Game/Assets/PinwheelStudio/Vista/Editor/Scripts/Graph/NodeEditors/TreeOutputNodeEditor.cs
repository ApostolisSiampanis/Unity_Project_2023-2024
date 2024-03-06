#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(TreeOutputNode))]
    public class TreeOutputNodeEditor : InstanceOutputNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent TREE_TEMPLATE = new GUIContent("Tree Template", "Template for the tree");
        private static readonly string VEGETATION_ASSETS_TEXT = "Add vegetation assets";

        private Texture GetPreviewTexture(INode node)
        {
            TreeOutputNode n = node as TreeOutputNode;
            if (n.treeTemplate != null && n.treeTemplate.prefab != null)
            {
                return AssetPreview.GetAssetPreview(n.treeTemplate.prefab);
            }
            else
            {
                return null;
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            nv.SetPreviewImage(GetPreviewTexture(node));
        }

        public override void OnGUI(INode node)
        {
            TreeOutputNode n = node as TreeOutputNode;
            EditorGUI.BeginChangeCheck();
            TreeTemplate template = EditorGUILayout.ObjectField(TREE_TEMPLATE, n.treeTemplate, typeof(TreeTemplate), false) as TreeTemplate;
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.treeTemplate = template;
            }
            base.OnGUI(node);

            EditorCommon.DrawAffLinks(VEGETATION_ASSETS_TEXT, Links.VEGETATION_ASSETS);
        }
    }
}
#endif
