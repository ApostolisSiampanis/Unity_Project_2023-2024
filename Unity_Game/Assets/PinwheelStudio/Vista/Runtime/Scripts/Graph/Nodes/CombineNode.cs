#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Combine",
        path = "General/Combine",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.8ip7e0lks9sg",
        keywords = "add, subtract, sub, mul, multiply, max, min, linear, lerp, overlay",
        description = "Combine 2 images with a specific operation.\n<ss>Tips: Search for the blend mode directly (eg: add, sub, mul, max, min, linear, dif, etc.).</ss>")]
    public class CombineNode : ImageNodeBase, ISetupWithHint
    {
        public readonly MaskSlot backgroundSlot = new MaskSlot("Background", SlotDirection.Input, 0);
        public readonly MaskSlot foregroundSlot = new MaskSlot("Foreground", SlotDirection.Input, 1);
        public readonly MaskSlot maskSlot = new MaskSlot("Mask", SlotDirection.Input, 2);

        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        public enum Mode
        {
            Add, Subtract, Multiply, Max, Min, Linear, Difference
        }

        [SerializeField]
        private Mode m_mode;
        public Mode mode
        {
            get
            {
                return m_mode;
            }
            set
            {
                m_mode = value;
            }
        }

        [SerializeField]
        private float m_backgroundMultiplier;
        public float backgroundMultiplier
        {
            get
            {
                return m_backgroundMultiplier;
            }
            set
            {
                m_backgroundMultiplier = Mathf.Clamp(value, 0f, 2f);
            }
        }

        [SerializeField]
        private float m_foregroundMultiplier;
        public float foregroundMultiplier
        {
            get
            {
                return m_foregroundMultiplier;
            }
            set
            {
                m_foregroundMultiplier = Mathf.Clamp(value, 0f, 2f);
            }
        }

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

        private Material m_material;

        private readonly string SHADER_NAME = "Hidden/Vista/Graph/Combine";
        private readonly int BACKGROUND = Shader.PropertyToID("_Background");
        private readonly int BACKGROUND_MULTIPLIER = Shader.PropertyToID("_BackgroundMultiplier");
        private readonly int FOREGROUND = Shader.PropertyToID("_Foreground");
        private readonly int FOREGROUND_MULTIPLIER = Shader.PropertyToID("_ForegroundMultiplier");
        private readonly int MASK = Shader.PropertyToID("_Mask");
        private readonly int MASK_MULTIPLIER = Shader.PropertyToID("_MaskMultiplier");
        private readonly int MODE = Shader.PropertyToID("_Mode");
        private readonly int PASS = 0;

        public CombineNode() : base()
        {
            m_backgroundMultiplier = 1;
            m_foregroundMultiplier = 1;
            m_maskMultiplier = 1;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef bgRefLink = context.GetInputLink(m_id, backgroundSlot.id);
            Texture background = context.GetTexture(bgRefLink);
            int inputResolution;
            if (background != null)
            {
                inputResolution = background.width;
            }
            else
            {
                background = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            SlotRef fgRefLink = context.GetInputLink(m_id, foregroundSlot.id);
            Texture foreground = context.GetTexture(fgRefLink);
            if (foreground == null)
            {
                foreground = Texture2D.blackTexture;
            }

            SlotRef maskRefLink = context.GetInputLink(m_id, maskSlot.id);
            Texture mask = context.GetTexture(maskRefLink);
            if (mask == null)
            {
                mask = Texture2D.whiteTexture;
            }

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(BACKGROUND, background);
            m_material.SetFloat(BACKGROUND_MULTIPLIER, backgroundMultiplier);
            m_material.SetTexture(FOREGROUND, foreground);
            m_material.SetFloat(FOREGROUND_MULTIPLIER, foregroundMultiplier);
            m_material.SetTexture(MASK, mask);
            m_material.SetFloat(MASK_MULTIPLIER, maskMultiplier);
            m_material.SetInt(MODE, (int)mode);

            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(bgRefLink);
            context.ReleaseReference(fgRefLink);
            context.ReleaseReference(maskRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public void SetupWithHint(string hint)
        {
            if (hint.Contains("add"))
            {
                m_mode = Mode.Add;
            }
            else if (hint.Contains("sub"))
            {
                m_mode = Mode.Subtract;
            }
            else if (hint.Contains("mul"))
            {
                m_mode = Mode.Multiply;
            }
            else if (hint.Contains("max"))
            {
                m_mode = Mode.Max;
            }
            else if (hint.Contains("min"))
            {
                m_mode = Mode.Min;
            }
            else if (hint.Contains("linear") || hint.Contains("lerp"))
            {
                m_mode = Mode.Linear;
            }
            else if (hint.Contains("dif") || hint.Contains("diff"))
            {
                m_mode = Mode.Difference;
            }
        }
    }
}
#endif
