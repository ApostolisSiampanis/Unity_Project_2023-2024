#if VISTA
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public abstract class ExecutableNodeBase : INode
    {
        [HideInInspector]
        [SerializeField]
        protected string m_groupId;
        [NonExposable]
        public string groupId
        {
            get
            {
                return m_groupId;
            }
            set
            {
                m_groupId = value;
            }
        }

        [HideInInspector]
        [SerializeField]
        protected string m_id;
        [NonExposable]
        public string id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        protected VisualState m_visualState;
        [NonExposable]
        public VisualState visualState
        {
            get
            {
                return m_visualState;
            }
            set
            {
                m_visualState = value;
            }
        }

        [SerializeField]
        protected bool m_shouldSplitExecution;
        public virtual bool shouldSplitExecution
        {
            get
            {
                return m_shouldSplitExecution;
            }
            set
            {
                m_shouldSplitExecution = value;
            }
        }

        [SerializeField]
        protected bool m_isBypassed;
        public virtual bool isBypassed
        {
            get
            {
                return m_isBypassed;
            }
            set
            {
                m_isBypassed = value;
            }
        }

        public ExecutableNodeBase() : base()
        {
            this.m_groupId = string.Empty;
            this.m_id = System.Guid.NewGuid().ToString();
            this.m_shouldSplitExecution = false;
            this.m_isBypassed = false;
        }

        public virtual ISlot[] GetInputSlots()
        {
            return GetSlots(SlotDirection.Input);
        }

        public virtual ISlot[] GetOutputSlots()
        {
            return GetSlots(SlotDirection.Output);
        }

        public virtual ISlot GetSlot(int id)
        {
            Type nodeType = GetType();
            FieldInfo[] fields = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo f = fields[i];
                if (typeof(ISlot).IsAssignableFrom(f.FieldType))
                {
                    ISlot slot = f.GetValue(this) as ISlot;
                    if (slot.id == id)
                    {
                        return slot;
                    }
                }
            }

            return null;
        }

        private ISlot[] GetSlots(SlotDirection slotType)
        {
            List<ISlot> slots = new List<ISlot>();
            Type nodeType = GetType();
            FieldInfo[] fields = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo f = fields[i];
                bool isSlot = typeof(ISlot).IsAssignableFrom(f.FieldType);
                if (isSlot)
                {
                    ISlot slot = f.GetValue(this) as ISlot;
                    if (slot.direction == slotType)
                    {
                        slots.Add(slot);
                    }
                }
            }

            return slots.ToArray();
        }

        public virtual IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public abstract void ExecuteImmediate(GraphContext context);

        public virtual void Bypass(GraphContext context)
        {
            if (this is IOutputNode)
            {
                throw new Exception("Should not bypass an output node");
            }

            ISlot[] inputSlots = GetInputSlots();
            ISlot firstInputSlot = null;
            if (inputSlots?.Length > 0)
            {
                firstInputSlot = inputSlots[0];
            }

            ISlot[] outputSlots = GetOutputSlots();
            ISlot firstOutputSlot = null;
            if (outputSlots?.Length > 0)
            {
                firstOutputSlot = outputSlots[0];
            }

            //The node is a generator (Noise, Shape, etc.) or input (Input, GetVar)
            if (firstInputSlot == null)
            {
                return;
            }

            //The node is a SetVar
            if (firstOutputSlot == null)
            {
                return;
            }

            //First IO slot have the same data type
            if (firstInputSlot.GetAdapter().slotType == firstOutputSlot.GetAdapter().slotType)
            {
                SlotRef inputRefLink = context.GetInputLink(m_id, firstInputSlot.id);
                string varName = inputRefLink.ToString();

                SlotRef outputRef = new SlotRef(m_id, firstOutputSlot.id);
                if (!string.IsNullOrEmpty(varName))
                {
                    if (!context.HasVariable(varName))
                    {
                        context.SetVariable(varName, inputRefLink);
                    }
                    context.LinkToVariable(outputRef, varName);
                }
                else
                {
                    context.LinkToInvalid(outputRef);
                }
            }

            //Other cases should be handle in the node override
        }

        public INode ShallowCopy()
        {
            return this.MemberwiseClone() as INode;
        }
    }
}
#endif
