#if VISTA
using Pinwheel.Vista.Graph;
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    public interface ITerrainGraphArgumentsFiller
    {
        void FillTerrainGraphArguments(TerrainGraph graph, IDictionary<int, Args> args);
    }
}
#endif
