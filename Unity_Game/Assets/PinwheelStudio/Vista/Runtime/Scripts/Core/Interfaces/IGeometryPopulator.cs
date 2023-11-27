#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IGeometryPopulator : ITile
    {
        void PopulateHeightMap(RenderTexture heightMap);
        void PopulateHoleMap(RenderTexture holeMap);
        void PopulateMeshDensityMap(RenderTexture meshDensityMap);
        void UpdateGeometry();
        void MatchSeams();
    }
}
#endif
