#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ObjectOutputNode))]
    public class ObjectOutputNodeEditor : InstanceOutputNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent OBJECT_TEMPLATE = new GUIContent("Object Template", "Template for the game object");
        private static readonly string PROPS_ASSET_TEXT = "Add decorative assets";

        private Texture GetPreviewTexture(INode node)
        {
            ObjectOutputNode n = node as ObjectOutputNode;
            if (n.objectTemplate != null && n.objectTemplate.prefab != null)
            {
                return AssetPreview.GetAssetPreview(n.objectTemplate.prefab);
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
            ObjectOutputNode n = node as ObjectOutputNode;
            EditorGUI.BeginChangeCheck();
            ObjectTemplate template = EditorGUILayout.ObjectField(OBJECT_TEMPLATE, n.objectTemplate, typeof(ObjectTemplate), false) as ObjectTemplate;
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.objectTemplate = template;
            }
            base.OnGUI(node);

            EditorCommon.DrawAffLinks(PROPS_ASSET_TEXT, Links.PROPS_ASSETS);
        }
    }
}
#endif
