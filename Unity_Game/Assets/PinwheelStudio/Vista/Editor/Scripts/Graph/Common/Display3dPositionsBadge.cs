#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Display3dPositionsBadge : Badge
    {
        public Display3dPositionsBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Display3dPositions");
            tooltip = "This node is displayed in 3D view as mask positions";
        }
    }
}
#endif
