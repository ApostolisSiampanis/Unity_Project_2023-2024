#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using System.Reflection;
using System;

namespace Pinwheel.Vista.ExposeProperty
{
    [System.Serializable]
    public class PropertyOverride
    {
        [SerializeField]
        private PropertyId m_id;
        public PropertyId id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        private int m_intValue;
        public int intValue
        {
            get
            {
                return m_intValue;
            }
            set
            {
                m_intValue = value;
            }
        }

        [SerializeField]
        private float m_floatValue;
        public float floatValue
        {
            get
            {
                return m_floatValue;
            }
            set
            {
                m_floatValue = value;
            }
        }

        [SerializeField]
        private bool m_boolValue;
        public bool boolValue
        {
            get
            {
                return m_boolValue;
            }
            set
            {
                m_boolValue = value;
            }
        }

        [SerializeField]
        private string m_stringValue;
        public string stringValue
        {
            get
            {
                return m_stringValue;
            }
            set
            {
                m_stringValue = value;
            }
        }

        [SerializeField]
        private Vector4 m_vectorValue;
        public Vector4 vectorValue
        {
            get
            {
                return m_vectorValue;
            }
            set
            {
                m_vectorValue = value;
            }
        }

        [SerializeField]
        private int m_enumValue;
        public int enumValue
        {
            get
            {
                return m_enumValue;
            }
            set
            {
                m_enumValue = value;
            }
        }


        [SerializeField]
        private Color m_colorValue;
        public Color colorValue
        {
            get
            {
                return m_colorValue;
            }
            set
            {
                m_colorValue = value;
            }
        }

        [SerializeField]
        private Gradient m_gradientValue;
        public Gradient gradientValue
        {
            get
            {
                return m_gradientValue;
            }
            set
            {
                m_gradientValue = value;
            }
        }

        [SerializeField]
        private AnimationCurve m_curveValue;
        public AnimationCurve curveValue
        {
            get
            {
                return m_curveValue;
            }
            set
            {
                m_curveValue = value;
            }
        }

        [SerializeField]
        private UnityEngine.Object m_objectValue;
        public UnityEngine.Object objectValue
        {
            get
            {
                return m_objectValue;
            }
            set
            {
                m_objectValue = value;
            }
        }

        public PropertyOverride(string nodeId, string propertyName)
        {
            PropertyId id = new PropertyId(nodeId, propertyName);
            m_id = id;
        }
    }
}
#endif
