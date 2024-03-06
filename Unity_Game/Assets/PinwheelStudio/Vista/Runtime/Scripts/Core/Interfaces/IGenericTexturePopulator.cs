#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IGenericTexturePopulator
    {
        void PopulateGenericTextures(List<string> labels, List<RenderTexture> textures);
    }
}
#endif
