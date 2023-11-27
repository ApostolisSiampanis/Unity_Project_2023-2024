#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    [CreateAssetMenu(menuName = "Vista/Position Container")]
    public class PositionContainer : ScriptableObject
    {
        [SerializeField]
        private PositionSample[] m_positions;
        /// <summary>
        /// Direct reference to the array
        /// </summary>
        public PositionSample[] positions
        {
            get
            {
                return m_positions;
            }
            set
            {
                m_positions = value;
            }
        }        
    }
}
#endif
