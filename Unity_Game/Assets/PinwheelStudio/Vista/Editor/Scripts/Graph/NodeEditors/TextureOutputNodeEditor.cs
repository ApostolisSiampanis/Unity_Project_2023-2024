#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(TextureOutputNode))]
    public class TextureOutputNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent TERRAIN_LAYER = new GUIContent("Terrain Layer", "The terrain layer associate with this output");
        private static readonly GUIContent ORDER = new GUIContent("Order", "The sorting order of this layer in the graph output. Some terrain shaders provide per-texture properties that require terrain textures to be in the correct order. If many nodes have the same order, they will be sorted using their position in the node list.");
        private static readonly string TEXTURE_ASSETS_TEXT = "Browse terrain textures";

        private Texture GetPreviewTexture(INode node)
        {
            TextureOutputNode n = node as TextureOutputNode;
            if (n.terrainLayer != null && n.terrainLayer.diffuseTexture != null)
            {
                return AssetPreview.GetAssetPreview(n.terrainLayer.diffuseTexture);
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
            TextureOutputNode n = node as TextureOutputNode;
            EditorGUI.BeginChangeCheck();
            TerrainLayer terrainLayer = EditorGUILayout.ObjectField(TERRAIN_LAYER, n.terrainLayer, typeof(TerrainLayer), false) as TerrainLayer;
            int order = EditorGUILayout.IntField(ORDER, n.order);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.terrainLayer = terrainLayer;
                n.order = order;
            }

            EditorCommon.DrawAffLinks(TEXTURE_ASSETS_TEXT, Links.TEXTURE_ASSETS);
        }
    }
}
#endif
