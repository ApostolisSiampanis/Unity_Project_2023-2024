#if VISTA
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Get Variable",
        path = "IO/Get Variable",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.fhz9v42v3ngc",
        keywords = "var",
        description = "Get a local variable in this graph. Use in parallel with Set Variable Node to stay organized.")]
    public class GetVariableNode : ExecutableNodeBase, IHasDependencyNodes, ISerializationCallbackReceiver, IHasDynamicSlotCount
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

        public GetVariableNode() : base()
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
            int id = 100;
            m_outputSlot = SlotProvider.Create(m_slotType, m_varName, SlotDirection.Output, id);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            if (m_outputSlot != null)
            {
                SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
                if (!string.IsNullOrEmpty(m_varName))
                {
                    context.LinkToVariable(outputRef, m_varName);
                }
                else
                {
                    context.LinkToInvalid(outputRef);
                }
            }
        }

        public IEnumerable<INode> GetDependencies(IEnumerable<INode> nodes)
        {
            if (string.IsNullOrEmpty(m_varName))
                return null;
            foreach (INode n in nodes)
            {
                if (n is SetVariableNode svn)
                {
                    if (m_varName.Equals(svn.varName))
                    {
                        return new INode[] { svn };
                    }
                }
            }
            return null;
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
