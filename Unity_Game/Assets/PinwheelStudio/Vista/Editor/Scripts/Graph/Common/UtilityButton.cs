#if VISTA
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class UtilityButton : Button
    {
        private Image m_image;
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

        public UtilityButton() : base()
        {
            Setup();
        }

        public UtilityButton(Action clickEvent) : base(clickEvent)
        {
            Setup();
        }

        private void Setup()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/UtilityButton");
            styleSheets.Add(uss);

            AddToClassList("utility-button");

            m_image = new Image();
            m_image.AddToClassList("image");
            Add(m_image);
        }

        public void SetToggled(bool t)
        {
            EnableInClassList("toggled", t);
        }
    }
}
#endif
