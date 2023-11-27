#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Display3dHeightBadge : Badge
    {
        public Display3dHeightBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Display3dHeight");
            tooltip = "This node is displayed in 3D view as height map";
        }
    }
}
#endif
