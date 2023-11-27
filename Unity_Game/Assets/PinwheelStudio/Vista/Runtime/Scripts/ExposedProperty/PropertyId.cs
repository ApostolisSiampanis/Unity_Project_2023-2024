#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using System;

namespace Pinwheel.Vista.ExposeProperty
{
    [System.Serializable]
    public class PropertyId : IEquatable<PropertyId>
    {
        [SerializeField]
        private string m_nodeId;
        public string nodeId
        {
            get
            {
                return m_nodeId;
            }
        }

        [SerializeField]
        private string m_propertyName;
        public string propertyName
        {
            get
            {
                return m_propertyName;
            }
        }

        public PropertyId(string nodeId, string propertyName)
        {
            this.m_nodeId = nodeId;
            this.m_propertyName = propertyName;
        }

        public override string ToString()
        {
            return $"{nodeId}.{propertyName}";
        }

        public bool Equals(PropertyId other)
        {
            return string.Equals(this.m_nodeId, other.m_nodeId) && string.Equals(propertyName, other.propertyName);
        }
    }
}
#endif
