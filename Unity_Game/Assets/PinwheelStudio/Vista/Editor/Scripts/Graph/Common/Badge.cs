#if VISTA
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Badge : VisualElement
    {
        protected Image m_icon;
        public Texture icon
        {
            get
            {
                if (m_icon != null)
                {
                    return m_icon.image;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (m_icon != null)
                {
                    m_icon.image = value;
                }
            }
        }

        protected VisualElement m_shadow;
        protected VisualElement m_bodyContainer;

        public Badge()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/Badge");
            styleSheets.Add(uss);

            m_bodyContainer = new VisualElement() { name = "body-container" };
            m_bodyContainer.AddToClassList("badge");
            this.Add(m_bodyContainer);

            m_shadow = new VisualElement() { name = "shadow" };
            m_shadow.pickingMode = PickingMode.Ignore;
            this.Add(m_shadow);
            m_shadow.SendToBack();

            m_icon = new Image() { name = "icon" };
            m_bodyContainer.Add(m_icon);
        }
    }
}
#endif
