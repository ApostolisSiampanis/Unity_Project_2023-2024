#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface ITerrainSystem
    {
        string terrainLabel { get; }
        Type GetTerrainComponentType();
        Type GetTileComponentType();
        ITile SetupTile(VistaManager manager, GameObject target);
    }
}
#endif
