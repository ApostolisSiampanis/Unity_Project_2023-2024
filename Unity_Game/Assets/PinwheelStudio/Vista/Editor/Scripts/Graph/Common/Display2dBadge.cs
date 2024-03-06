#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Display2dBadge : Badge
    {
        public Display2dBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Display2D");
            tooltip = "This node is displayed in 2D view";
        }
    }
}
#endif
