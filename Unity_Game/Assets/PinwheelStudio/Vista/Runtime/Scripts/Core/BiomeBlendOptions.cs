#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    [System.Serializable]
    public struct BiomeBlendOptions
    {
        [System.Serializable]
        public enum TextureBlendMode
        {
            Linear, Additive, Subtractive, Max, Min
        }

        [System.Serializable]
        public enum BufferBlendMode
        {
            Linear, Additive
        }

        [SerializeField]
        private TextureBlendMode m_heightMapBlendMode;
        public TextureBlendMode heightMapBlendMode
        {
            get
            {
                return m_heightMapBlendMode;
            }
            set
            {
                m_heightMapBlendMode = value;
            }
        }

        //[SerializeField]
        //private TextureBlendMode m_textureBlendMode;
        //public TextureBlendMode textureBlendMode
        //{
        //    get
        //    {
        //        return m_textureBlendMode;
        //    }
        //    set
        //    {
        //        m_textureBlendMode = value;
        //    }
        //}

        [SerializeField]
        private TextureBlendMode m_detailDensityBlendMode;
        public TextureBlendMode detailDensityBlendMode
        {
            get
            {
                return m_detailDensityBlendMode;
            }
            set
            {
                m_detailDensityBlendMode = value;
            }
        }

        [SerializeField]
        private BufferBlendMode m_instancesBlendMode;
        public BufferBlendMode instancesBlendMode
        {
            get
            {
                return m_instancesBlendMode;
            }
            set
            {
                m_instancesBlendMode = value;
            }
        }

        public static BiomeBlendOptions Default()
        {
            BiomeBlendOptions options = new BiomeBlendOptions();
            options.heightMapBlendMode = TextureBlendMode.Linear;
            //options.textureBlendMode = TextureBlendMode.Linear;
            options.detailDensityBlendMode = TextureBlendMode.Linear;
            options.instancesBlendMode = BufferBlendMode.Linear;

            return options;
        }
    }
}
#endif
