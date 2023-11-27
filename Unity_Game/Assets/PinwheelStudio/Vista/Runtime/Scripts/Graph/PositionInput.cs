#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public class PositionInput
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
        private PositionContainer m_positionContainer;
        public PositionContainer positionContainer
        {
            get
            {
                return m_positionContainer;
            }
            set
            {
                m_positionContainer = value;
            }
        }
    }
}
#endif
