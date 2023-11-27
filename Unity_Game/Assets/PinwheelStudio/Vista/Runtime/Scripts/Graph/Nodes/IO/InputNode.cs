#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Input",
        path = "IO/Graph Input",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.45tfsdccgixu",
        keywords = "",
        description = "A generic entry point of the graph which will be used to take data from the outside into the graph with sub-graph or in code.\nThis will create a corresponding slot in sub-graph node.")]
    public class InputNode : ExecutableNodeBase, IHasDynamicSlotCount, ISerializationCallbackReceiver
    {
        [System.NonSerialized]
        private ISlot m_outputSlot;
        public ISlot outputSlot
        {
            get
            {
                return m_outputSlot;
            }
        }

        [SerializeField]
        private Serializer.JsonObject m_outputSlotData;

        [SerializeField]
        private string m_inputName;
        public string inputName
        {
            get
            {
                return m_inputName;
            }
            set
            {
                m_inputName = value;
            }
        }

        [System.NonSerialized]
        private Type m_slotType;
        public Type slotType
        {
            get
            {
                return m_slotType;
            }
        }

        [SerializeField]
        private string m_slotTypeName;

        public event IHasDynamicSlotCount.SlotsChangedHandler slotsChanged;

        public InputNode() : base()
        {
            m_slotType = typeof(MaskSlot);
            CreateOutputSlot();
        }

        public void SetSlotType(Type t)
        {
            Type oldValue = m_slotType;
            Type newValue = t;
            m_slotType = newValue;
            if (oldValue != newValue)
            {
                CreateOutputSlot();
                if (slotsChanged != null)
                {
                    slotsChanged.Invoke(this);
                }
            }
        }

        private void CreateOutputSlot()
        {
            int id = 100;
            m_outputSlot = SlotProvider.Create(m_slotType, m_inputName, SlotDirection.Output, id);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            if (!string.IsNullOrEmpty(m_inputName) && m_outputSlot != null)
            {
                SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
                context.SetExternal(outputRef, inputName);
            }
        }

        public void OnBeforeSerialize()
        {
            if (m_outputSlot != null)
            {
                m_outputSlotData = Serializer.Serialize<ISlot>(m_outputSlot);
            }
            else
            {
                m_outputSlotData = default;
            }

            if (m_slotType != null)
            {
                m_slotTypeName = m_slotType.FullName;
            }
        }

        public void OnAfterDeserialize()
        {
            if (!m_outputSlotData.Equals(default))
            {
                m_outputSlot = Serializer.Deserialize<ISlot>(m_outputSlotData);
            }
            else
            {
                m_outputSlot = null;
            }

            if (!string.IsNullOrEmpty(m_slotTypeName))
            {
                m_slotType = Type.GetType(m_slotTypeName);
            }
        }

        public override ISlot[] GetInputSlots()
        {
            return new ISlot[0];
        }

        public override ISlot[] GetOutputSlots()
        {
            if (m_outputSlot != null)
            {
                return new ISlot[1] { m_outputSlot };
            }
            else
            {
                return new ISlot[0];
            }
        }

        public override ISlot GetSlot(int id)
        {
            if (m_outputSlot != null && m_outputSlot.id == id)
            {
                return m_outputSlot;
            }
            return null;
        }
    }
}
#endif
