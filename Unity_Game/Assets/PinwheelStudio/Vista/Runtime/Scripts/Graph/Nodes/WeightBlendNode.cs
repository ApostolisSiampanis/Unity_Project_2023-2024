#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Weight Blend",
        path = "General/Weight Blend",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.fj8gf67dmpnz",
        keywords = "",
        description = "An easy way to blend texture weight maps. Weight maps will be stacked layer by layer; lower layers will be adjusted to ensure their sum is not overshoot.\nDefault value for the first layer is white, other layers is black.")]
    public class WeightBlendNode : ImageNodeBase, IHasDynamicSlotCount
    {
        [SerializeField]
        private MaskSlot[] m_inputSlots;
        public MaskSlot[] inputSlots
        {
            get
            {
                return m_inputSlots;
            }
        }

        [SerializeField]
        private MaskSlot[] m_outputSlots;
        public MaskSlot[] outputSlots
        {
            get
            {
                return m_outputSlots;
            }
        }

        [SerializeField]
        private int m_layerCount;
        [NonExposable]
        public int layerCount
        {
            get
            {
                return m_layerCount;
            }
            set
            {
                int oldValue = m_layerCount;
                int newValue = Mathf.Clamp(value, MIN_LAYER, MAX_LAYER);
                m_layerCount = newValue;
                if (oldValue != newValue)
                {
                    UpdateSlotArrays();
                    if (slotsChanged != null)
                    {
                        slotsChanged.Invoke(this);
                    }
                }
            }
        }

        public const int MIN_LAYER = 2;
        public const int MAX_LAYER = 100;

        public event IHasDynamicSlotCount.SlotsChangedHandler slotsChanged;

        public WeightBlendNode() : base()
        {
            m_layerCount = 2;
            UpdateSlotArrays();
        }

        private void UpdateSlotArrays()
        {
            m_inputSlots = new MaskSlot[m_layerCount];
            for (int i = 0; i < m_layerCount; ++i)
            {
                m_inputSlots[i] = new MaskSlot($"Layer {i}", SlotDirection.Input, i);
            }

            m_outputSlots = new MaskSlot[m_layerCount];
            for (int i = 0; i < m_layerCount; ++i)
            {
                m_outputSlots[i] = new MaskSlot($"Layer {i}", SlotDirection.Output, MAX_LAYER + i);
            }
        }

        public override ISlot[] GetInputSlots()
        {
            ISlot[] slots = new ISlot[m_inputSlots.Length];
            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i] = m_inputSlots[i];
            }
            return slots;
        }

        public override ISlot[] GetOutputSlots()
        {
            ISlot[] slots = new ISlot[m_outputSlots.Length];
            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i] = m_outputSlots[i];
            }
            return slots;
        }

        public override ISlot GetSlot(int id)
        {
            MaskSlot[] slots = id < MAX_LAYER ? m_inputSlots : m_outputSlots;
            for (int i = 0; i < slots.Length; ++i)
            {
                if (slots[i].id == id)
                    return slots[i];
            }
            return null;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef[] inputRefLinks = new SlotRef[m_inputSlots.Length];
            Texture[] inputTextures = new Texture[m_inputSlots.Length];
            for (int i = 0; i < inputRefLinks.Length; ++i)
            {
                SlotRef refLink = context.GetInputLink(m_id, m_inputSlots[i].id);
                inputRefLinks[i] = refLink;
                Texture inputTexture = context.GetTexture(refLink);
                if (inputTexture == null)
                {
                    inputTexture = i == 0 ? Texture2D.whiteTexture : Texture2D.blackTexture;
                }
                inputTextures[i] = inputTexture;
            }
            int inputResolution;
            if (inputTextures[0] == Texture2D.whiteTexture)
            {
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputTextures[0].width;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);

            RenderTexture[] targetRts = new RenderTexture[m_outputSlots.Length];
            for (int i = 0; i < targetRts.Length; ++i)
            {
                SlotRef outputRef = new SlotRef(m_id, m_outputSlots[i].id);
                if (context.GetReferenceCount(outputRef) > 0)
                {
                    targetRts[i] = context.CreateRenderTarget(desc, outputRef);
                }
            }

            WeightsBlend.Blend(targetRts, inputTextures);

            for (int i = 0; i < inputRefLinks.Length; ++i)
            {
                context.ReleaseReference(inputRefLinks[i]);
            }
        }

        public override void Bypass(GraphContext context)
        {
            for (int i = 0; i < m_inputSlots.Length; ++i)
            {
                SlotRef inputRefLink = context.GetInputLink(m_id, m_inputSlots[i].id);
                string varName = inputRefLink.ToString();

                SlotRef outputRef = new SlotRef(m_id, m_outputSlots[i].id);
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
        }
    }
}
#endif
