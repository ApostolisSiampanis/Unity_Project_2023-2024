#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public interface IStickyNote: IHasID, IGroupable
    {
        string title { get; set; }
        string contents { get; set; }
        int fontSize { get; set; }
        int theme { get; set; }
        Rect position { get; set; }
    }
}
#endif
