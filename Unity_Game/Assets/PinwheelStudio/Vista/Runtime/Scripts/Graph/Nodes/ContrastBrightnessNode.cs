#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Contrast & Brightness",
        path = "Adjustments/Contrast & Brightness",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.nhdpqwewefqv",
        keywords = "contrast, brightness",
        description = "Apply a contrast & brightness filter on the image. Similar to other image processing software.")]
    public class ContrastBrightnessNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_contrast;
        public float contrast
        {
            get
            {
                return m_contrast;
            }
            set
            {
                m_contrast = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        private float m_brightness;
        public float brightness
        {
            get
            {
                return m_brightness;
            }
            set
            {
                m_brightness = Mathf.Clamp(value, -1, 1);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/ContrastBrightness";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int CONTRAST = Shader.PropertyToID("_Contrast");
        private static readonly int BRIGHTNESS = Shader.PropertyToID("_Brightness");
        private static readonly int PASS = 0;

        private Material m_material;

        public ContrastBrightnessNode() : base()
        {
            m_contrast = 0f;
            m_brightness = 0f;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            Texture inputTexture = context.GetTexture(inputRefLink);
            int inputResolution;
            if (inputTexture == null)
            {
                inputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputTexture.width;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(CONTRAST, m_contrast);
            m_material.SetFloat(BRIGHTNESS, m_brightness);

            Drawing.DrawQuad(targetRt, m_material, PASS);
            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }
    }
}
#endif
