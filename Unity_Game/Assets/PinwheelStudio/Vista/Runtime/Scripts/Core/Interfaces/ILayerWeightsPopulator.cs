#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface ILayerWeightsPopulator : ITile
    {
        TerrainLayer[] terrainLayers { get; }
        void PopulateLayerWeights(List<TerrainLayer> layers, List<RenderTexture> weights);
    }
}
#endif
