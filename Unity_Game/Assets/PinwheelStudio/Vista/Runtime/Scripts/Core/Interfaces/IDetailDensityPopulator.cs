#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IDetailDensityPopulator : ITile
    {
        ProgressiveTask PopulateDetailDensity(List<DetailTemplate> templates, List<RenderTexture> densityMaps);
    }
}
#endif
