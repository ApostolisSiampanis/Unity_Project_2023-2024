#if VISTA
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pinwheel.Vista.Graph
{
    [Obsolete]
    [System.Serializable]
    public struct ObjectRef : IEquatable<ObjectRef>
    {
        [SerializeField]
        private string m_guid;
        public string guid
        {
            get
            {
                return m_guid;
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

        public ObjectRef(Object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            m_guid = Guid.NewGuid().ToString();
            m_target = target;
        }

        public bool Equals(ObjectRef other)
        {
            return this.m_guid.Equals(other.m_guid) && this.m_target.Equals(other.m_target);
        }
    }
}
#endif
