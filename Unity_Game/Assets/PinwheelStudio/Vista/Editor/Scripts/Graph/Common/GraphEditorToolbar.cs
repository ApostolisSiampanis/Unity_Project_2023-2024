#if VISTA
using Pinwheel.VistaEditor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class GraphEditorToolbar : VisualElement
    {
        private GraphEditorBase m_editor;
        public VisualElement leftContainer { get; set; }
        public VisualElement rightContainer { get; set; }

        private ToolbarButton m_saveButton;
        private ToolbarButton m_saveAsButton;
        private ToolbarButton m_showInProjectButton;

        public GraphEditorToolbar(GraphEditorBase editor)
        {
            this.m_editor = editor;
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/Toolbar");
            styleSheets.Add(uss);

            leftContainer = new VisualElement() { name = "left-container" };
            rightContainer = new VisualElement() { name = "right-container" };
            this.Add(leftContainer);
            this.Add(rightContainer);

            m_saveButton = new ToolbarButton() { text = "Save Asset" };
            m_saveButton.clicked += OnSaveButtonClicked;
            leftContainer.Add(m_saveButton);
            leftContainer.Add(new VerticalSeparator());

            m_saveAsButton = new ToolbarButton() { text = "Save As..." };
            m_saveAsButton.clicked += OnSaveAsButtonClicked;
            leftContainer.Add(m_saveAsButton);
            leftContainer.Add(new VerticalSeparator());

            m_showInProjectButton = new ToolbarButton() { text = "Show In Project" };
            m_showInProjectButton.clicked += OnShowInProjectButtonClicked;
            leftContainer.Add(m_showInProjectButton);
            leftContainer.Add(new VerticalSeparator());
        }

        private void OnSaveButtonClicked()
        {
            m_editor.OnSaveRequest();
        }

        private void OnSaveAsButtonClicked()
        {
            m_editor.OnSaveAsRequest();
        }

        private void OnShowInProjectButtonClicked()
        {
            EditorGUIUtility.PingObject(m_editor.sourceGraph);
        }

        internal void OnGUI()
        {
            
        }
    }
}
#endif
