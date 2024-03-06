#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IAlbedoMapPopulator : ITile
    {
        void PopulateAlbedoMap(RenderTexture albedoMap);
    }
}
#endif
