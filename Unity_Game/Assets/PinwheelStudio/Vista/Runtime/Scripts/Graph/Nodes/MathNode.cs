#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Math",
        path = "General/Math",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.mrg7xq2w4kgg",
        keywords = "add, sub, subtract, mul, multiply, pow, power, abs, absolute, math, number, value, invert, one minus, oneminus, one, sin, cos",
        description = "Perform simple numerical math operations on the texture.\n<ss>Tips: Search for the operation directly (eg: add, sub, mul, pow, abs, minus, oneminus, etc.).</ss>")]
    public partial class MathNode : ImageNodeBase, ISerializationCallbackReceiver, ISetupWithHint
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        public const int MAX_CONFIG = 4;

        [SerializeField]
        private List<MathConfig> m_maths;
        public List<MathConfig> maths
        {
            get
            {
                return m_maths;
            }
            set
            {
                m_maths = value;
            }
        }

        private Material m_material;

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Math";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int CONFIGS = Shader.PropertyToID("_Configs");

        public MathNode() : base()
        {
            m_maths = new List<MathConfig>();
            m_maths.Add(new MathConfig());
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

            Vector4[] configArray = new Vector4[4];
            int configCount = Mathf.Min(m_maths.Count, MAX_CONFIG);
            bool isHeavy = false;
            for (int i = 0; i < configCount; ++i)
            {
                configArray[i] = m_maths[i].ToVector();
                isHeavy = isHeavy || (m_maths[i].ops == Operator.Pow) || (m_maths[i].ops == Operator.Sin) || (m_maths[i].ops == Operator.Cos);
            }
            m_material.SetVectorArray(CONFIGS, configArray);

            int pass = isHeavy ? 0 : 1;
            Drawing.DrawQuad(targetRt, m_material, pass);

            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public void SetupWithHint(string hint)
        {
            m_maths = new List<MathConfig>();
            if (hint.Contains("add"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 0.5f,
                    ops = Operator.Add
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("sub"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = -0.5f,
                    ops = Operator.Add
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("mul"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 0.5f,
                    ops = Operator.Multiply
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("pow"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 2f,
                    ops = Operator.Pow
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("abs"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 1,
                    ops = Operator.Abs
                };
                m_maths.Add(c);
            }
            else if (hint.Equals("one"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 1f,
                    ops = Operator.Add
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("oneminus") || hint.Contains("one minus") || hint.Contains("invert"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 1f,
                    ops = Operator.OneMinus
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("minus"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = -1f,
                    ops = Operator.Multiply
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("sin"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 1,
                    ops = Operator.Sin
                };
                m_maths.Add(c);
            }
            else if (hint.Contains("cos"))
            {
                MathConfig c = new MathConfig()
                {
                    enabled = true,
                    number = 1,
                    ops = Operator.Cos
                };
                m_maths.Add(c);
            }
        }
    }

    public partial class MathNode
    {
        public enum Operator
        {
            Add, Multiply, Pow, Abs, OneMinus, Sin, Cos
        }

        [System.Serializable]
        public class MathConfig
        {
            [SerializeField]
            protected bool m_enabled;
            public bool enabled
            {
                get
                {
                    return m_enabled;
                }
                set
                {
                    m_enabled = value;
                }
            }

            [SerializeField]
            private float m_number;
            public float number
            {
                get
                {
                    return m_number;
                }
                set
                {
                    m_number = value;
                }
            }

            [SerializeField]
            private Operator m_ops;
            public Operator ops
            {
                get
                {
                    return m_ops;
                }
                set
                {
                    m_ops = value;
                }
            }

            public MathConfig()
            {
                m_enabled = true;
                m_number = 0;
                m_ops = Operator.Add;
            }

            public Vector4 ToVector()
            {
                Vector4 v = new Vector4();
                v.x = m_enabled ? 1 : 0;
                v.y = m_number;
                v.z = (int)m_ops;
                v.w = 0;
                return v;
            }
        }

        public void OnBeforeSerialize()
        {
            if (m_maths.Count > MAX_CONFIG)
            {
                m_maths.RemoveRange(MAX_CONFIG, m_maths.Count - MAX_CONFIG);
            }
        }

        public void OnAfterDeserialize()
        {

        }
    }
}
#endif
