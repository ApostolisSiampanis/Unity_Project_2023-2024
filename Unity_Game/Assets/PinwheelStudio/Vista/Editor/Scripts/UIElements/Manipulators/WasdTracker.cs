#if VISTA
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class WasdTracker : MouseManipulator
    {
        public struct WasdInfo
        {
            public bool isW;
            public bool isA;
            public bool isS;
            public bool isD;
            public bool isQ;
            public bool isE;

            public bool isShift;
        }

        public bool isActive { get; private set; }

        private bool m_isW;
        private bool m_isA;
        private bool m_isS;
        private bool m_isD;
        private bool m_isQ;
        private bool m_isE;
        private bool m_isShift;

        public WasdInfo wasdInfo
        {
            get
            {
                WasdInfo info = new WasdInfo();
                info.isW = m_isW;
                info.isA = m_isA;
                info.isS = m_isS;
                info.isD = m_isD;
                info.isQ = m_isQ;
                info.isE = m_isE;
                info.isShift = m_isShift;

                return info;
            }
        }

        public WasdTracker()
        {
        }

        protected override void RegisterCallbacksOnTarget()
        { 
            target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.NoTrickleDown);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.NoTrickleDown);
            target.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.NoTrickleDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.NoTrickleDown);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.NoTrickleDown);
            target.UnregisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.NoTrickleDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.NoTrickleDown);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 1)
            {
                target.CaptureMouse();
                isActive = true;
                e.StopPropagation();
            }
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (isActive)
            {
                m_isW |= e.keyCode == KeyCode.W;
                m_isA |= e.keyCode == KeyCode.A;
                m_isS |= e.keyCode == KeyCode.S;
                m_isD |= e.keyCode == KeyCode.D;
                m_isQ |= e.keyCode == KeyCode.Q;
                m_isE |= e.keyCode == KeyCode.E;
                m_isShift |= e.shiftKey;

                e.StopPropagation();
            }
        }

        private void OnKeyUp(KeyUpEvent e)
        {
            m_isW &= e.keyCode != KeyCode.W;
            m_isA &= e.keyCode != KeyCode.A;
            m_isS &= e.keyCode != KeyCode.S;
            m_isD &= e.keyCode != KeyCode.D;
            m_isQ &= e.keyCode != KeyCode.Q;
            m_isE &= e.keyCode != KeyCode.E;
            m_isShift &= e.shiftKey;
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (target.HasMouseCapture())
            {
                target.ReleaseMouse();
                e.StopPropagation();
            }
            isActive = false;
            m_isW = false;
            m_isA = false;
            m_isS = false;
            m_isD = false;
            m_isQ = false;
            m_isE = false;
            m_isShift = false;
        }
    }
}
#endif