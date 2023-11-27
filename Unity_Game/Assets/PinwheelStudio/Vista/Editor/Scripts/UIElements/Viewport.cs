#if VISTA
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Viewport : VisualElement
    {
        protected Image m_image;
        public Texture image
        {
            get
            {
                if (m_image != null)
                {
                    return m_image.image;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (m_image != null)
                {
                    m_image.image = value;
                }
            }
        }

        public Viewport()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/Viewport");
            styleSheets.Add(uss);
            AddToClassList("viewport");

            m_image = new Image();
            m_image.scaleMode = ScaleMode.ScaleToFit;
            m_image.AddToClassList("viewport-image");
            this.Add(m_image);
        }
    }
}
#endif
