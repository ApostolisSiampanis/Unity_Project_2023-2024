#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Color Blend",
        path = "General/Color Blend",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.7pdo1vvp7r3l",
        keywords = "",
        description = "Perform an alpha blend of several color layers.\nDefault value for the first slot is white, other slots is black.")]
    public class ColorBlendNode : ImageNodeBase, IHasDynamicSlotCount
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

        public readonly ColorTextureSlot outputSlot = new ColorTextureSlot("Output", SlotDirection.Output, MAX_LAYER);

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

        [SerializeField]
        private Color[] m_layerColors;
        public Color[] layerColors
        {
            get
            {
                return m_layerColors;
            }
        }

        public const int MIN_LAYER = 1;
        public const int MAX_LAYER = 100;

        public event IHasDynamicSlotCount.SlotsChangedHandler slotsChanged;

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/ColorBlend";
        private static readonly string TEMP_RT_NAME = "Temp";
        private static readonly int COLOR = Shader.PropertyToID("_Color");
        private static readonly int WEIGHT = Shader.PropertyToID("_Weight");
        private static readonly int BACKGROUND = Shader.PropertyToID("_Background");
        private static readonly int PASS = 0;

        private Material m_material;

        public ColorBlendNode() : base()
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

            if (m_layerColors != null)
            {
                Color[] colors = new Color[m_layerCount];
                colors.Fill(Color.white);

                int count = Mathf.Min(m_layerColors.Length, colors.Length);
                System.Array.Copy(m_layerColors, colors, count);
                m_layerColors = colors;
            }
            else
            {
                m_layerColors = new Color[m_layerCount];
                m_layerColors.Fill(Color.white);
            }
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
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, RenderTextureFormat.ARGB32);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            RenderTexture tempRt = context.CreateTemporaryRT(desc, TEMP_RT_NAME);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            for (int i = 0; i < inputTextures.Length; ++i)
            {
                Drawing.Blit(targetRt, tempRt);
                m_material.SetTexture(BACKGROUND, tempRt);
                m_material.SetTexture(WEIGHT, inputTextures[i]);
                m_material.SetColor(COLOR, m_layerColors[i]);
                Drawing.DrawQuad(targetRt, m_material, PASS);
            }

            context.ReleaseTemporary(TEMP_RT_NAME);
            for (int i = 0; i < inputRefLinks.Length; ++i)
            {
                context.ReleaseReference(inputRefLinks[i]);
            }
            Object.DestroyImmediate(m_material);
        }

        public override ISlot[] GetInputSlots()
        {
            return m_inputSlots;
        }

        public override ISlot[] GetOutputSlots()
        {
            return new ISlot[] { outputSlot };
        }

        public override ISlot GetSlot(int id)
        {
            if (id < MAX_LAYER)
            {
                foreach (ISlot s in m_inputSlots)
                {
                    if (s.id == id)
                    {
                        return s;
                    }
                }
            }
            if (outputSlot.id == id)
            {
                return outputSlot;
            }
            return null;
        }
    }
}
#endif
