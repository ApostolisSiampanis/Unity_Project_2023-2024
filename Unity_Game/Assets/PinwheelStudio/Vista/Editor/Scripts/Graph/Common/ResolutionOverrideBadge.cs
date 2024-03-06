#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class ResolutionOverrideBadge : Badge
    {
        public ResolutionOverrideBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/ResolutionOverride");
            tooltip = "This node is overriding its base resolution";
        }
    }
}
#endif
