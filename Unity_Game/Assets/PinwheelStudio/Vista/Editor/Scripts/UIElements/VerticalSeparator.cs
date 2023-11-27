#if VISTA
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class VerticalSeparator : VisualElement
    {
        public VerticalSeparator()
        {
            style.width = new StyleLength(1);
            style.height = new StyleLength(StyleKeyword.Auto);
            style.marginLeft = new StyleLength(3);
            style.marginRight = new StyleLength(3);
            style.backgroundColor = new StyleColor(new Color32(35, 35, 35, 255));
        }
    }
}
#endif
