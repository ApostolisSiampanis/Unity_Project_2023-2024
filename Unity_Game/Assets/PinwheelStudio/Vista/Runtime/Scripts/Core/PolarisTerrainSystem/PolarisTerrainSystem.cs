#if VISTA
#if GRIFFIN
using UnityEngine;
using System;
using Pinwheel.Griffin;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Vista.PolarisTerrain
{
    public class PolarisTerrainSystem : ITerrainSystem
    {
        public string terrainLabel
        {
            get
            {
                return "Polaris Terrain";
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void OnInitialize()
        {
            VistaManager.RegisterTerrainSystem<PolarisTerrainSystem>();
        }

        public Type GetTerrainComponentType()
        {
            return typeof(GStylizedTerrain);
        }

        public Type GetTileComponentType()
        {
            return typeof(PolarisTile);
        }

        public ITile SetupTile(VistaManager manager, GameObject target)
        {
            GStylizedTerrain terrainComponent = target.GetComponent<GStylizedTerrain>();
            if (terrainComponent == null)
            {
                return null;
            }
            PolarisTile tile = target.GetComponent<PolarisTile>();
            if (tile == null)
            {
#if UNITY_EDITOR
                tile = Undo.AddComponent<PolarisTile>(target);
#else
                tile = target.AddComponent<PolarisTile>();
#endif
            }
#if UNITY_EDITOR
            Undo.RecordObject(tile, "Modify Polaris Tile");
#endif
            tile.managerId = manager.id;
            return tile;
        }
    }
}
#endif
#endif