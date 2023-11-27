#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.Vista
{
    public interface IProceduralBiome : IBiome
    {
        TerrainGraph terrainGraph { get; set; }
    }
}
#endif