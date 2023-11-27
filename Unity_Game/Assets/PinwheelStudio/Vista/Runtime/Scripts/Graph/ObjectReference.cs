#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace Pinwheel.Vista.Graph
{
    [Serializable]
    public struct ObjectReference
    {
        [SerializeField]
        private string m_key;
        public string key
        {
            get
            {
                return m_key;
            }
        }

        [SerializeField]
        private Object m_target;
        public Object target
        {
            get
            {
                return m_target;
            }
        }

        public ObjectReference(string key, Object target)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"{nameof(key)} should not be null or empty");
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            m_key = key;
            m_target = target;
        }
    }
}
#endif
