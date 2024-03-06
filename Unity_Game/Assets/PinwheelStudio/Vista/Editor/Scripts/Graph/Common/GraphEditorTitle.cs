#if VISTA
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public class GraphEditorTitle
    {
        private GraphEditorBase m_editor;

        public GraphEditorTitle(GraphEditorBase editor)
        {
            this.m_editor = editor;
        }

        public void OnGUI()
        {
            if (m_editor.sourceGraph != null && m_editor.clonedGraph != null)
            {
                bool isModified = EditorUtility.IsDirty(m_editor.clonedGraph);
                string title = string.Format("{0}{1}", isModified ? "*" : "", m_editor.sourceGraph.name);
                string assetPath = AssetDatabase.GetAssetPath(m_editor.sourceGraph);
                Texture icon = null;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    icon = AssetDatabase.GetCachedIcon(assetPath);
                }
                m_editor.titleContent = new GUIContent(title, icon);
            }
            else if (m_editor.sourceGraph == null && m_editor.clonedGraph != null)
            {
                string title = string.Format("{0}{1}", m_editor.clonedGraph.name, "*");
                m_editor.titleContent = new GUIContent(title);
            }
            else
            {
                m_editor.titleContent = new GUIContent();
            }
        }
    }
}
#endif
