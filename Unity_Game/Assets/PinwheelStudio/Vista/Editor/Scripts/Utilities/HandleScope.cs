#if VISTA
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.VistaEditor
{
    public struct HandleScope : IDisposable
    {
        private Color m_oldColor;
        private CompareFunction m_oldCompareFunction;

        public HandleScope(Color c, CompareFunction f)
        {
            m_oldColor = Handles.color;
            m_oldCompareFunction = Handles.zTest;

            Handles.color = c;
            Handles.zTest = f;
        }

        public void Dispose()
        {
            Handles.color = m_oldColor;
            Handles.zTest = m_oldCompareFunction;
        }
    }
}
#endif
