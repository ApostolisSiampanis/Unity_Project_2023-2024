#if VISTA
using System;
using UnityEditor;

namespace Pinwheel.VistaEditor
{
    public struct LabelWidthScope : IDisposable
    {
        private float m_oldWidth;

        public LabelWidthScope(float w)
        {
            m_oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = w;
        }

        public void Dispose()
        {
            EditorGUIUtility.labelWidth = m_oldWidth;
        }
    }
}
#endif
