#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class XMarkBadge : Badge
    {
        public XMarkBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/XMark");
            tooltip = null;
        }
    }
}
#endif
