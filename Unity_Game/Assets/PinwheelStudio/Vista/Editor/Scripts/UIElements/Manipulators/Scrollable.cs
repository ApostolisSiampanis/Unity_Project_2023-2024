#if VISTA
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{
    public class Scrollable : MouseManipulator
    {
        public Action<Vector2> onScroll;

        public Scrollable(Action<Vector2> scrollCallback)
        {
            onScroll = scrollCallback;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(OnMouseWheelEvent, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnMouseWheelEvent, TrickleDown.NoTrickleDown);
        }

        private void OnMouseWheelEvent(WheelEvent e)
        {
            if (onScroll != null)
            {
                onScroll.Invoke(e.delta);
                e.StopPropagation();
            }
        }
    }
}
#endif