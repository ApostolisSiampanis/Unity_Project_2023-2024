#if VISTA
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Thin Out",
        path = "General/Thin Out",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ts04dru5by9t",
        keywords = "snip, cull, thin, position",
        description = "Remove some samples from the set using a mask. The blacker the mask, the higher change that the samples will be removed.")]
    public class ThinOutNode : ExecutableNodeBase, IHasSeed
    {
        public readonly BufferSlot positionInputSlot = new BufferSlot("Position", SlotDirection.Input, 0);
        public readonly MaskSlot maskSlot = new MaskSlot("Mask", SlotDirection.Input, 1);
        public readonly BufferSlot positionOutputSlot = new BufferSlot("Position", SlotDirection.Output, 100);

        [SerializeField]
        private float m_maskMultiplier;
        public float maskMultiplier
        {
            get
            {
                return m_maskMultiplier;
            }
            set
            {
                m_maskMultiplier = Mathf.Clamp(value, 0f, 2f);
            }
        }

        [SerializeField]
        private int m_seed;
        public int seed
        {
            get
            {
                return m_seed;
            }
            set
            {
                m_seed = value;
            }
        }

        public ThinOutNode() : base()
        {
            m_maskMultiplier = 1f;
            m_seed = 0;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            SlotRef inputPositionSlotRefLink = context.GetInputLink(m_id, positionInputSlot.id);
            ComputeBuffer inputPositionBuffer = context.GetBuffer(inputPositionSlotRefLink);

            SlotRef maskSlotRefLink = context.GetInputLink(m_id, maskSlot.id);
            Texture maskTexture = context.GetTexture(maskSlotRefLink);

            if (inputPositionBuffer == null) 
            {
                //do nothing but don't return here
            }
            else if (inputPositionBuffer?.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse position buffer, node id {m_id}");
            }
            else
            {
                SlotRef outputRef = new SlotRef(m_id, positionOutputSlot.id);
                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(inputPositionBuffer.count);
                ComputeBuffer outputBuffer = context.CreateBuffer(desc, outputRef);

                NodeLibraryUtilities.ThinOutNode.Execute(context, inputPositionBuffer, maskTexture, m_maskMultiplier, m_seed, outputBuffer);
            }

            context.ReleaseReference(inputPositionSlotRefLink);
            context.ReleaseReference(maskSlotRefLink);
        }
    }
}
#endif
