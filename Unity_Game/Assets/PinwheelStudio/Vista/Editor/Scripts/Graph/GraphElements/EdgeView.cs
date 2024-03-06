#if VISTA
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class EdgeView : Edge
    {
        public string edgeId { get; set; }

        public EdgeView() : base()
        {
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == 0)
            {
                GraphEditorGraphView graphView = this.GetFirstAncestorOfType<GraphEditorGraphView>();
                if (graphView != null)
                {
                    Vector2 mousePos = this.ChangeCoordinatesTo(graphView.contentViewContainer, evt.localMousePosition);
                    graphView.InvokeEdgeDoubleClickedCallback(this, mousePos);
                }
            }
        }
    }
}
#endif
