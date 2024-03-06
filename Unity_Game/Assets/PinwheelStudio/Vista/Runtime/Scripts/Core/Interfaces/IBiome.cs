#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IBiome
    {
        GameObject gameObject { get; }
        int order { get; set; }
        long updateCounter { get; set; }
        BiomeBlendOptions blendOptions { get; }
        BiomeDataRequest RequestData(Bounds worldBounds, int heightMapResolution, int textureResolution);
        bool IsOverlap(Bounds area);
        void OnBeforeVMGenerate();
        void OnAfterVMGenerate();

    }
}
#endif