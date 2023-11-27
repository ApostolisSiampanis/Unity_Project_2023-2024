#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public delegate void PopulateGenericTexturesHandler(List<string> labels, List<RenderTexture> textures);
    public delegate void PopulateGenericBuffersHandler(List<string> labels, List<ComputeBuffer> buffers);
    public delegate void PopulatePrefabHandler(ITile tile, GameObject spawnedGO);
}
#endif
