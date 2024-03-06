#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public interface IGroup : IHasID
    {
        string title { get; set; }
        Rect position { get; set; }
    }
}
#endif
