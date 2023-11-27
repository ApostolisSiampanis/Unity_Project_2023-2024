#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class BypassBadge : Badge
    {
        public BypassBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Bypass");
            tooltip = "This node is bypassed";
        }
    }
}
#endif
