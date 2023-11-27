#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    public static class TileExtensions
    {
        public static bool IsOverlap(this ITile tile, IBiome[] biomes)
        {
            foreach (IBiome b in biomes)
            {
                if (b.IsOverlap(tile.worldBounds))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool OverlapTest(this ITile tile, IBiome[] biomes, HashSet<KeyValuePair<ITile, IBiome>> result)
        {
            bool overlapped = false;
            foreach (IBiome b in biomes)
            {
                if (b.IsOverlap(tile.worldBounds))
                {
                    result.Add(new KeyValuePair<ITile, IBiome>(tile, b));
                    overlapped = true;
                }
            }
            return overlapped;
        }
    }
}
#endif
