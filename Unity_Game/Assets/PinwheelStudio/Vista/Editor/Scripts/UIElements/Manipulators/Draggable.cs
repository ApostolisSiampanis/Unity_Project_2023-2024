#if VISTA
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Draggable : MouseManipulator
    {
        public struct DragInfo
        {
            public int button { get; set; }
            public Vector2 delta { get; set; }
            public bool isShift { get; set; }
            public bool isCtrl { get; set; }
            public bool isAlt { get; set; }
        }

        public Action<DragInfo> onDrag { get; set; }
        public bool isDragging { get; private set; }

        private int m_button;

        public Draggable(Action<DragInfo> dragCallback)
        {
            onDrag = dragCallback;
            m_button = -1;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.NoTrickleDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.NoTrickleDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.NoTrickleDown);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            m_button = e.button;
            target.CaptureMouse();
            isDragging = true;
            e.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!isDragging)
                return;
            if (onDrag != null)
            {
                DragInfo d = new DragInfo();
                d.button = m_button;
                d.delta = e.mouseDelta;
                d.isShift = e.shiftKey;
                d.isCtrl = e.ctrlKey;
                d.isAlt = e.altKey;
                onDrag.Invoke(d);
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (target.HasMouseCapture())
            {
                target.ReleaseMouse();
                e.StopPropagation();
            }
            isDragging = false;
            m_button = -1;
        }
    }
}
#endif