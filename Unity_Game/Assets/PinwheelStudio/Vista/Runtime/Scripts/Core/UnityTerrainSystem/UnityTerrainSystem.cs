#if VISTA
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Vista.UnityTerrain
{
    public class UnityTerrainSystem : ITerrainSystem
    {
        public string terrainLabel
        {
            get
            {
                return "Unity Terrain";
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        public static void OnInitialize()
        {
            VistaManager.RegisterTerrainSystem<UnityTerrainSystem>();
        }

        public Type GetTerrainComponentType()
        {
            return typeof(Terrain);
        }

        public Type GetTileComponentType()
        {
            return typeof(TerrainTile);
        }

        public ITile SetupTile(VistaManager manager, GameObject target)
        {
            Terrain terrain = target.GetComponent<Terrain>();
            if (terrain == null)
            {
                return null;
            }
            TerrainTile tile = target.GetComponent<TerrainTile>();
            if (tile == null)
            {
#if UNITY_EDITOR
                tile = Undo.AddComponent<TerrainTile>(target);
#else
                tile = target.AddComponent<TerrainTile>();
#endif
            }
#if UNITY_EDITOR
            Undo.RecordObject(tile, "Modify Terrain Tile");
#endif
            tile.managerId = manager.id;
            return tile;
        }
    }
}
#endif
