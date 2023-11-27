#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(DetailInstanceOutputNode))]
    public class DetailInstanceOutputNodeEditor : InstanceOutputNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent DETAIL_TEMPLATE = new GUIContent("Detail Template", "Template for the detail");
        private static readonly string VEGETATION_ASSETS_TEXT = "Add vegetation assets";

        private Texture GetPreviewTexture(INode node)
        {
            DetailInstanceOutputNode n = node as DetailInstanceOutputNode;
            Texture preview = null;
            if (n.detailTemplate != null)
            {
                DetailTemplate t = n.detailTemplate;
                if (t.renderMode == DetailRenderMode.Grass || t.renderMode == DetailRenderMode.GrassBillboard)
                {
                    if (t.texture != null)
                    {
                        preview = AssetPreview.GetAssetPreview(t.texture);
                    }
                }
                else if (t.renderMode == DetailRenderMode.VertexLit)
                {
                    if (t.prefab != null)
                    {
                        preview = AssetPreview.GetAssetPreview(t.prefab);
                    }
                }
            }

            return preview;
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            nv.SetPreviewImage(GetPreviewTexture(node));
        }

        public override void OnGUI(INode node)
        {
            DetailInstanceOutputNode n = node as DetailInstanceOutputNode;
            EditorGUI.BeginChangeCheck();
            DetailTemplate template = EditorGUILayout.ObjectField(DETAIL_TEMPLATE, n.detailTemplate, typeof(DetailTemplate), false) as DetailTemplate;
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.detailTemplate = template;
            }
            base.OnGUI(node);
            EditorCommon.DrawAffLinks(VEGETATION_ASSETS_TEXT, Links.VEGETATION_ASSETS);
        }
    }
}
#endif
