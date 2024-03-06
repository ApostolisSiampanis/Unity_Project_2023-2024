#if VISTA
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_2021_1_OR_NEWER
using ProgressBar = UnityEngine.UIElements.ProgressBar;
#else
using ProgressBar = UnityEditor.UIElements.ProgressBar;
#endif

namespace Pinwheel.VistaEditor.Graph
{
    public class GraphEditorStatusBar : VisualElement
    {
        private GraphEditorBase m_editor;

        public VisualElement leftContainer { get; set; }
        public VisualElement rightContainer { get; set; }

        public ProgressBar totalProgressBar { get; set; }
        public ProgressBar currentProgressBar { get; set; }

        private Label m_infoRightLabel;
        public string infoRight
        {
            get
            {
                if (m_infoRightLabel != null)
                {
                    return m_infoRightLabel.text;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (m_infoRightLabel != null)
                {
                    m_infoRightLabel.text = value;
                }
            }
        }

        public GraphEditorStatusBar(GraphEditorBase editor)
        {
            this.m_editor = editor;

            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/StatusBar");
            styleSheets.Add(uss);

            leftContainer = new VisualElement() { name = "left-container" };
            rightContainer = new VisualElement() { name = "right-container" };
            this.Add(leftContainer);
            this.Add(rightContainer);

            totalProgressBar = new ProgressBar() { name = "total-progress" };
            currentProgressBar = new ProgressBar() { name = "current-progress" };
            leftContainer.Add(totalProgressBar);
            leftContainer.Add(currentProgressBar);
            SetProgressVisible(false);

            m_infoRightLabel = new Label() { name = "info-right-label" };
            rightContainer.Add(m_infoRightLabel);
        }

        public void SetProgress(float total, float current)
        {
            totalProgressBar.value = total * 100f;
            currentProgressBar.value = current * 100f;
        }

        public void SetProgressVisible(bool visible)
        {
            totalProgressBar.visible = visible;
            currentProgressBar.visible = visible;
        }

        public bool IsProgressVisible()
        {
            return totalProgressBar.visible || currentProgressBar.visible;
        }
    }
}
#endif
