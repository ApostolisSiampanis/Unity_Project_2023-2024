#if VISTA
using System;
using UnityEditor;

namespace Pinwheel.VistaEditor
{
    public struct IndentScope : IDisposable
    {
        private int m_oldIndent;

        public IndentScope(int i)
        {
            m_oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = i;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = m_oldIndent;
        }
    }
}
#endif
