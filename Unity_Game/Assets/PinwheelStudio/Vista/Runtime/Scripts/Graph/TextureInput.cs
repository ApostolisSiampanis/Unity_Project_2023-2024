#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class TextureInput
    {
        [SerializeField]
        private string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        [SerializeField]
        private Texture2D m_texture;
        public Texture2D texture
        {
            get
            {
                return m_texture;
            }
            set
            {
                m_texture = value;
            }
        }
    }
}
#endif
