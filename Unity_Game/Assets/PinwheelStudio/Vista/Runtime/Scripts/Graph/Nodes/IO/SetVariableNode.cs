#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Set Variable",
        path = "IO/Set Variable",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.a89r8myuy84g",
        keywords = "var",
        description = "Register a local variable in the graph.\nUse in parallel with Get Variable node to stay organized.")]
    public class SetVariableNode : ExecutableNodeBase, ISerializationCallbackReceiver, IHasDynamicSlotCount
    {
        [System.NonSerialized]
        private ISlot m_inputSlot;
        public ISlot inputSlot
        {
            get
            {
                return m_inputSlot;
            }
        }

        [SerializeField]
        private Serializer.JsonObject m_inputSlotData;

        [SerializeField]
        private string m_varName;
        public string varName
        {
            get
            {
                return m_varName;
            }
            set
            {
                m_varName = value;
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

        public SetVariableNode() : base()
        {
            m_slotType = typeof(MaskSlot);
            CreateSlot();
        }

        public void SetSlotType(Type t)
        {
            Type oldValue = m_slotType;
            Type newValue = t;
            m_slotType = newValue;
            if (oldValue != newValue)
            {
                CreateSlot();
                if (slotsChanged != null)
                {
                    slotsChanged.Invoke(this);
                }
            }
        }

        private void CreateSlot()
        {
            int id = 0;
            m_inputSlot = SlotProvider.Create(m_slotType, m_varName, SlotDirection.Input, id);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            if (!string.IsNullOrEmpty(m_varName))
            {
                SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
                context.SetVariable(m_varName, inputRefLink);
            }
        }

        public void OnBeforeSerialize()
        {
            if (m_inputSlot != null)
            {
                m_inputSlotData = Serializer.Serialize<ISlot>(m_inputSlot);
            }
            else
            {
                m_inputSlotData = default;
            }

            if (m_slotType != null)
            {
                m_slotTypeName = m_slotType.FullName;
            }
        }

        public void OnAfterDeserialize()
        {
            if (!m_inputSlotData.Equals(default))
            {
                m_inputSlot = Serializer.Deserialize<ISlot>(m_inputSlotData);
            }
            else
            {
                m_inputSlot = null;
            }

            if (!string.IsNullOrEmpty(m_slotTypeName))
            {
                m_slotType = Type.GetType(m_slotTypeName);
            }
        }

        public override ISlot[] GetInputSlots()
        {
            if (m_inputSlot != null)
            {
                return new ISlot[] { m_inputSlot };
            }
            else
            {
                return new ISlot[] { };
            }
        }

        public override ISlot[] GetOutputSlots()
        {
            return new ISlot[] { };
        }

        public override ISlot GetSlot(int id)
        {
            if (m_inputSlot != null && m_inputSlot.id == id)
            {
                return m_inputSlot;
            }
            return null;
        }
    }
}
#endif
