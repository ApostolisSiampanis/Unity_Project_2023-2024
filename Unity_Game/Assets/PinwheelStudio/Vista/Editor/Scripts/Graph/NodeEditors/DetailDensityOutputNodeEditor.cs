#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(DetailDensityOutputNode))]
    public class DetailDensityOutputNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent DETAIL_TEMPLATE = new GUIContent("Detail Template", "Template asset for this detail. Right click on the Project window, select Vista>Detail Template to create one");
        private static readonly GUIContent DENSITY_MULTIPLIER = new GUIContent("Density Multiplier", "Multiplier for detail density");
        private static readonly string VEGETATION_ASSETS_TEXT = "Add vegetation assets"; 

        private Texture GetPreviewTexture(INode node)
        {
            DetailDensityOutputNode n = node as DetailDensityOutputNode;
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
            DetailDensityOutputNode n = node as DetailDensityOutputNode;
            EditorGUI.BeginChangeCheck();
            DetailTemplate detailTemplate = EditorGUILayout.ObjectField(DETAIL_TEMPLATE, n.detailTemplate, typeof(DetailTemplate), false) as DetailTemplate;
            float densityMultiplier = EditorGUILayout.Slider(DENSITY_MULTIPLIER, n.densityMultiplier, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.detailTemplate = detailTemplate;
                n.densityMultiplier = densityMultiplier;
            }
            EditorCommon.DrawAffLinks(VEGETATION_ASSETS_TEXT, Links.VEGETATION_ASSETS);
        }
    }
}
#endif
