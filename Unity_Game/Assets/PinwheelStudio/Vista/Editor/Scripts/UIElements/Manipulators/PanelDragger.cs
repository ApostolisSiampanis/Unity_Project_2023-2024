#if VISTA
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.UIElements
{

    public class PanelDragger : MouseManipulator
    {
        private Vector2 m_start;
        protected bool m_active;
        protected VisualElement m_panel;
        public bool clampToParentEdges { get; set; }

        public PanelDragger(VisualElement panel)
        {
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
            clampToParentEdges = false;
            m_active = false;
            m_panel = panel;
        }

        /// <summary>
        ///   <para>Calculate new position of the dragged element.</para>
        /// </summary>
        /// <param name="x">New x position.</param>
        /// <param name="y">New y position.</param>
        /// <param name="width">Element width.</param>
        /// <param name="height">Element height.</param>
        /// <returns>
        ///   <para>Calculated and validated position.</para>
        /// </returns>
        protected Rect CalculatePosition(float x, float y, float width, float height)
        {
            Rect result = new Rect(x, y, width, height);
            if (clampToParentEdges)
            {
                Rect rect = m_panel.parent.contentRect;
                if (result.x < rect.xMin)
                {
                    result.x = rect.xMin;
                }
                else if (result.xMax > rect.xMax)
                {
                    result.x = rect.xMax - result.width;
                }
                if (result.y < rect.yMin)
                {
                    result.y = rect.yMin;
                }
                else if (result.yMax > rect.yMax)
                {
                    result.y = rect.yMax - result.height;
                }
                result.width = width;
                result.height = height;
            }
            return result;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        /// <summary>
        ///   <para>Called on mouse down event.</para>
        /// </summary>
        /// <param name="e">The event.</param>
        protected void OnMouseDown(MouseDownEvent e)
        {
            if (m_active)
            {
                e.StopImmediatePropagation();
                return;
            }
            if (CanStartManipulation(e))
            {
                m_start = e.localMousePosition;
                m_active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        /// <summary>
        ///   <para>Called on mouse move event.</para>
        /// </summary>
        /// <param name="e">The event.</param>
        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (m_panel != null && m_active)
            {
                Vector2 vector = e.localMousePosition - m_start;
                if (m_panel != null)
                {
                    Vector3 scale = m_panel.transform.scale;
                    vector.x *= scale.x;
                    vector.y *= scale.y;
                }
                Rect layout = CalculatePosition(m_panel.layout.x + vector.x, m_panel.layout.y + vector.y, m_panel.layout.width, m_panel.layout.height);
                //if (base.target.isLayoutManual)
                //{
                //	base.target.layout = layout;
                //}
                //else 
                if (m_panel.resolvedStyle.position == Position.Absolute)
                {
                    m_panel.style.left = layout.x;
                    m_panel.style.top = layout.y;
                }
                e.StopPropagation();
            }
        }

        /// <summary>
        ///   <para>Called on mouse up event.</para>
        /// </summary>
        /// <param name="e">The event.</param>
        protected void OnMouseUp(MouseUpEvent e)
        {
            if (m_active && CanStopManipulation(e))
            {
                m_active = false;
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }
    }
}
#endif