#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Display3dMaskBadge : Badge
    {
        public Display3dMaskBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Display3dMask");
            tooltip = "This node is displayed in 3D view as mask map";
        }
    }
}
#endif
