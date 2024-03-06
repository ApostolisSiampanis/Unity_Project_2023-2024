#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface ISceneHeightProvider
    {
        void OnCollectSceneHeight(RenderTexture targetRt, Rect requestedWorldRect);
    }
}
#endif
