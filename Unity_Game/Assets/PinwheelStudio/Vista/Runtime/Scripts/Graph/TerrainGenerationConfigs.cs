#if VISTA
using System.ComponentModel;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public struct TerrainGenerationConfigs
    {
        [SerializeField]
        private int m_resolution;
        public int resolution
        {
            get
            {
                return m_resolution;
            }
            set
            {
                m_resolution = Utilities.MultipleOf8(Mathf.Clamp(value, Constants.RES_MIN, Constants.RES_MAX));
            }
        }

        [SerializeField]
        private Rect m_worldBounds;
        public Rect worldBounds
        {
            get
            {
                return m_worldBounds;
            }
            set
            {
                m_worldBounds = value;
            }
        }

        [SerializeField]
        private float m_terrainHeight;
        public float terrainHeight
        {
            get
            {
                return m_terrainHeight;
            }
            set
            {
                m_terrainHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int m_seed;
        public int seed
        {
            get
            {
                return m_seed;
            }
            set
            {
                m_seed = value;
            }
        }

        [SerializeField]
        private bool m_shouldOutputTempHeight;
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool shouldOutputTempHeight
        {
            get
            {
                return m_shouldOutputTempHeight;
            }
            set
            {
                m_shouldOutputTempHeight = value;
            }
        }

        public static TerrainGenerationConfigs Create()
        {
            TerrainGenerationConfigs configs = new TerrainGenerationConfigs();
            configs.resolution = Constants.K1024;
            configs.worldBounds = new Rect(0, 0, 1000, 1000);
            configs.terrainHeight = 600;
            configs.seed = 1234;
            configs.shouldOutputTempHeight = false;
            return configs;
        }
    }
}
#endif
