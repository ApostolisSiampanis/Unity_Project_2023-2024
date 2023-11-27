#if VISTA
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Resizer : VisualElement
    {
        public enum Position
        {
            Left, Bottom
        }

        public Position position { get; private set; }

        protected VisualElement m_line;

        public Resizer(Position pos)
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Resizer");
            styleSheets.Add(uss);
            AddToClassList("resizer");

            position = pos;
            this.AddManipulator(new Draggable(OnDrag));

            m_line = new VisualElement() { name = "Line" };
            m_line.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            this.Add(m_line);

            RegisterCallback<MouseOverEvent>(OnMouseOver);
            RegisterCallback<MouseOutEvent>(OnMouseOut);
        }

        public void AttachTo(VisualElement target)
        {
            target.Add(this);
            if (position == Position.Left)
            {
                AddToClassList("left");
                m_line.AddToClassList("line-left");
            }
            else if (position == Position.Bottom)
            {
                AddToClassList("bottom");
                m_line.AddToClassList("line-bottom");
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        private void OnDrag(Draggable.DragInfo info)
        {
            if (parent == null)
                return;
            if (parent.resolvedStyle == null)
                return;
            if (position == Position.Left)
            {
                float width = parent.resolvedStyle.width;
                width -= info.delta.x;
                parent.style.width = new StyleLength(width);
                parent.MarkDirtyRepaint();
            }
            else if (position == Position.Bottom)
            {
                float height = parent.resolvedStyle.height;
                height += info.delta.y;
                parent.style.height = new StyleLength(height);
                parent.MarkDirtyRepaint();
            }
        }

        private void OnMouseOver(MouseOverEvent evt)
        {
            m_line.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        private void OnMouseOut(MouseOutEvent evt)
        {
            m_line.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}
#endif
