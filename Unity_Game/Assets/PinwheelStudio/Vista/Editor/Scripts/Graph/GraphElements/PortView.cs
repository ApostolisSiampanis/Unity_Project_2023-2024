#if VISTA
using Pinwheel.Vista.Graph;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using EdgeViewBase = UnityEditor.Experimental.GraphView.Edge;

namespace Pinwheel.VistaEditor.Graph
{
    public class PortView : Port
    {
        private static readonly Color32 COLOR_GENERIC = new Color32(193, 193, 193, 255);
        private static readonly Color32 COLOR_MASK = new Color32(132, 228, 231, 255);
        private static readonly Color32 COLOR_TEXTURE = new Color32(251, 203, 244, 255);
        private static readonly Color32 COLOR_BUFFER = new Color32(246, 255, 154, 255);

        private ISlotAdapter m_adapter;
        public ISlotAdapter adapter
        {
            get
            {
                return m_adapter;
            }
            set
            {
                m_adapter = value;
                if (m_adapter != null)
                {
                    Type t = m_adapter.slotType;
                    if (t == typeof(MaskSlot))
                    {
                        portColor = COLOR_MASK;
                    }
                    else if (t == typeof(ColorTextureSlot))
                    {
                        portColor = COLOR_TEXTURE;
                    }
                    else if (t == typeof(BufferSlot))
                    {
                        portColor = COLOR_BUFFER;
                    }
                    else if (t == typeof(GenericSlot))
                    {
                        portColor = COLOR_GENERIC;
                    }
                }
            }
        }

        public SlotRef slotRef { get; set; }
        public Type slotType { get; set; }

        protected PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            allowMultiDrag = true;
        }

        public static PortView Create<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener edgeConnectorListener) where TEdge : EdgeViewBase, new()
        {
            PortView port = new PortView(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(edgeConnectorListener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        public VisualElement GetConnectorBox()
        {
            return m_ConnectorBox;
        }
    }
}
#endif
