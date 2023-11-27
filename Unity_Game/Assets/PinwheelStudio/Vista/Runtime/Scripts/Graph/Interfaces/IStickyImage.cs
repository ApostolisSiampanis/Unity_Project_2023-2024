#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public interface IStickyImage: IHasID, IGroupable
    {
        string textureGuid { get; set; }
        Rect position { get; set; }
    }
}
#endif
