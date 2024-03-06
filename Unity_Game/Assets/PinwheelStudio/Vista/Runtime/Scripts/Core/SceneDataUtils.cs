#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class SceneDataUtils
    {
        public static void CollectWorldHeight(ITile[] tiles, RenderTexture targetRt, Bounds worldBounds)
        {
            Rect worldRect = new Rect(worldBounds.min.x, worldBounds.min.z, worldBounds.size.x, worldBounds.size.z);
            for (int i = 0; i < tiles.Length; ++i)
            {
                ITile t = tiles[i];

                Bounds tileBounds = t.worldBounds;
                Rect tileRect = new Rect(tileBounds.min.x, tileBounds.min.z, tileBounds.size.x, tileBounds.size.z);
                if (!tileRect.Overlaps(worldRect))
                    return;

                if (t is ISceneHeightProvider shp)
                {
                    shp.OnCollectSceneHeight(targetRt, worldRect);
                }
            }
        }


    }
}
#endif
