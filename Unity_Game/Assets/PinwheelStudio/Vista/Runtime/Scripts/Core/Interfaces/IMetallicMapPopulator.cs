#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IMetallicMapPopulator : ITile
    {
        void PopulateMetallicMap(RenderTexture metallicMap);
    }
}
#endif
