#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista.Graph
{
    [System.Obsolete]
    public interface IHasAssetReferences
    {
        void OnSerializeAssetReferences(List<ObjectRef> refs);
        void OnDeserializeAssetReferences(List<ObjectRef> refs);
    }
}
#endif
