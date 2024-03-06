#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using System;
using System.Reflection;
using Pinwheel.Vista.Graph;

namespace Pinwheel.Vista.ExposeProperty
{
    [System.Serializable]
    public class PropertyDescriptor
    {
        [SerializeField]
        internal PropertyId m_id;
        public PropertyId id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        internal string m_label;
        public string label
        {
            get
            {
                return m_label;
            }
            set
            {
                m_label = value;
            }
        }

        [SerializeField]
        internal string m_description;
        public string description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
            }
        }

        [SerializeField]
        internal string m_groupName;
        public string groupName
        {
            get
            {
                return m_groupName;
            }
            set
            {
                m_groupName = value;
            }
        }

        [SerializeField]
        internal string m_enumTypeName;
        public Type enumType
        {
            get
            {
                return Type.GetType(m_enumTypeName);
            }
        }

        [SerializeField]
        internal string m_objectTypeName;
        public Type objectType
        {
            get
            {
                return !string.IsNullOrEmpty(m_objectTypeName) ? Type.GetType(m_objectTypeName) : null;
            }
        }

        [SerializeField]
        internal PropertyType m_propertyType;
        public PropertyType propertyType
        {
            get
            {
                return m_propertyType;
            }
        }

        [SerializeField]
        internal MinMaxInt m_intValueRange;
        public MinMaxInt intValueRange
        {
            get
            {
                return m_intValueRange;
            }
            set
            {
                m_intValueRange = value;
            }
        }

        [SerializeField]
        internal MinMaxFloat m_floatValueRange;
        public MinMaxFloat floatValueRange
        {
            get
            {
                return m_floatValueRange;
            }
            set
            {
                m_floatValueRange = value;
            }
        }

        internal PropertyDescriptor(GraphAsset graph, string nodeId, string propertyName)
        {
            INode node = graph.GetNode(nodeId);
            if (node == null)
                throw new System.Exception($"Failed to create exposed property. Node {nodeId} not found.");
            PropertyInfo propertyInfo = node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            if (propertyInfo == null)
                throw new System.Exception($"Cannot expose property {propertyName} for node {node.id}. Property not found.");
            if (!IsExposable(propertyInfo.PropertyType))
                throw new System.Exception($"Property {propertyInfo.Name} of type {propertyInfo.PropertyType.Name} is not exposable.");

            m_id = new PropertyId(nodeId, propertyName);
            m_intValueRange = MinMaxInt.FULL_RANGE;
            m_floatValueRange = MinMaxFloat.FULL_RANGE;
        }

        public static bool IsExposable(Type t)
        {
            if (t.IsEnum)
                return true;

            if (t.IsSubclassOf(typeof(UnityEngine.Object)))
                return true;

            if (t == typeof(int) ||
                t == typeof(float) ||
                t == typeof(bool) ||
                t == typeof(string) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Vector4) ||
                t == typeof(Color) ||
                t == typeof(Color32) ||
                t == typeof(Gradient) ||
                t == typeof(AnimationCurve))
                return true;

            return false;
        }

        
    }
}
#endif
