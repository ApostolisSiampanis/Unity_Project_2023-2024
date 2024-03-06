#if VISTA
using Pinwheel.Vista.Graph;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using EdgeViewBase = UnityEditor.Experimental.GraphView.Edge;

namespace Pinwheel.VistaEditor.Graph
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphEditorGraphView m_graphView;

        private GraphViewChange m_graphViewChange;

        private List<EdgeViewBase> m_edgesToCreate;

        private List<GraphElement> m_edgesToDelete;

        private List<GraphElement> m_movedElements;

        public Port lastStartPort { get; private set; }
        public Vector2 lastDroppedPosition { get; private set; }

        public EdgeConnectorListener(GraphEditorGraphView graphView)
        {
            m_graphView = graphView;
            m_edgesToCreate = new List<EdgeViewBase>();
            m_edgesToDelete = new List<GraphElement>();
            m_movedElements = new List<GraphElement>();
            m_graphViewChange.edgesToCreate = m_edgesToCreate;
        }

        public void OnDropOutsidePort(EdgeViewBase edge, Vector2 position)
        {
            if (edge.input != null && edge.output != null)
                return;
            if (edge.output != null)
            {
                lastStartPort = edge.output;
            }
            else if (edge.input != null)
            {
                lastStartPort = edge.input;
            }
            else
            {
                lastStartPort = null;
            }

            lastDroppedPosition = position;

            NodeCreationContext context = new NodeCreationContext();
            context.target = null;
            context.index = -1;
            context.screenMousePosition = position + m_graphView.m_editor.position.position;
            m_graphView.nodeCreationRequest.Invoke(context);
        }

        public void OnDrop(GraphView graphView, EdgeViewBase edge)
        {
            m_edgesToCreate.Clear();
            m_edgesToCreate.Add(edge);
            m_edgesToDelete.Clear();
            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (EdgeView connection in edge.input.connections)
                {
                    if (connection != edge)
                    {
                        m_edgesToDelete.Add(connection); 
                    }
                }
            }
            if (edge.output.capacity == Port.Capacity.Single)
            {
                foreach (EdgeView connection2 in edge.output.connections)
                {
                    if (connection2 != edge)
                    {
                        m_edgesToDelete.Add(connection2);
                    }
                }
            }
            if (m_edgesToDelete.Count > 0)
            {
                graphView.DeleteElements(m_edgesToDelete);
            }
            List<EdgeViewBase> edgesToCreate = m_edgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_graphViewChange).edgesToCreate;
            }
            foreach (EdgeView item in edgesToCreate)
            {
                graphView.AddElement(item);
                edge.input.Connect(item);
                edge.output.Connect(item);
            }
        }

        public void ResetState()
        {
            lastStartPort = null;
            lastDroppedPosition = Vector2.zero;
        }

        public void ConnectToLastStartPort(NodeView nv, int slotId)
        {
            Port candidate = null;
            nv.Query<Port>().ForEach(p =>
            {
                SlotRef slotRef = (p as PortView).slotRef;
                if (slotRef.slotId == slotId)
                {
                    candidate = p;
                }
            });

            if (candidate == null)
                return;

            EdgeView edge = lastStartPort.ConnectTo<EdgeView>(candidate);
            OnDrop(m_graphView, edge);

            EditorCoroutineUtility.StartCoroutine(OffsetNodeDelayed(nv, candidate), this);
        }

        private IEnumerator OffsetNodeDelayed(NodeView nv, Port p)
        {
            yield return null;
            PortView pv = p as PortView;
            VisualElement connectorBox = pv.GetConnectorBox();
            Vector2 offset = pv.ChangeCoordinatesTo(nv, connectorBox.layout.center);

            Rect currentNodePos = nv.GetPosition();
            currentNodePos.position -= offset;
            nv.SetPosition(currentNodePos);
            m_graphView.SyncNodePosition(nv, currentNodePos);

            EditorUtility.SetDirty(m_graphView.m_editor.clonedGraph);
        }
    }
}
#endif
