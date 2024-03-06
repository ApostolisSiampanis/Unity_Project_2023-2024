#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface ITile
    {
        string managerId { get; set; }
        GameObject gameObject { get; }
        Bounds worldBounds { get; }
        float maxHeight { get; set; }
        int heightMapResolution { get; set; }
        int textureResolution { get; set; }
        int detailDensityMapResolution { get; set; }
        void OnBeforeApplyingData();
        void OnAfterApplyingData();
    }
}
#endif
