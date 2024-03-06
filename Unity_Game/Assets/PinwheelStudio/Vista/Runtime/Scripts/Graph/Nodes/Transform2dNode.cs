#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Transform 2D",
        path = "General/Transform 2D",
        icon = "",
        documentation = "",
        keywords = "offset, rotation, scale, flip",
        description = "Perform some 2D transformation on an image.")]
    public class Transform2dNode : ImageNodeBase, ISerializationCallbackReceiver, IHasDynamicSlotCount
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

        public enum TilingMode
        {
            None, TileX, TileY, TileXY
        }

        [SerializeField]
        private TilingMode m_tilingMode;
        public TilingMode tilingMode
        {
            get
            {
                return m_tilingMode;
            }
            set
            {
                m_tilingMode = value;
            }
        }

        [SerializeField]
        private Vector2 m_offset;
        public Vector2 offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        [SerializeField]
        private float m_rotation;
        public float rotation
        {
            get
            {
                return m_rotation;
            }
            set
            {
                m_rotation = value;
            }
        }

        [SerializeField]
        private Vector2 m_scale;
        public Vector2 scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
            }
        }

        [SerializeField]
        private Color m_backgroundColor;
        public Color backgroundColor
        {
            get
            {
                return m_backgroundColor;
            }
            set
            {
                m_backgroundColor = value;
            }
        }

        public static readonly string SHADER_NAME = "Hidden/Vista/Graph/Transform2D";
        public static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        public static readonly int TRANSFORM_MATRIX = Shader.PropertyToID("_TransformMatrix");
        public static readonly int BACKGROUND_COLOR = Shader.PropertyToID("_BackgroundColor");

        public static readonly string TILE_X = "TILE_X";
        public static readonly string TILE_Y = "TILE_Y";
        public static readonly int PASS = 0;
        public static readonly Vector2[] UVS = new Vector2[] { new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, 1), new Vector2(1, -1) };

        private Material m_material;

        public Transform2dNode() : base()
        {
            m_slotType = typeof(MaskSlot);
            CreateSlot();
            OnCreate();
        }

        public Transform2dNode(Type slotType) : base()
        {
            m_slotType = slotType;
            CreateSlot();
            OnCreate();
        }

        private void OnCreate()
        {
            m_tilingMode = TilingMode.TileXY;
            m_offset = Vector2.zero;
            m_rotation = 0;
            m_scale = Vector2.one * 100;
            m_backgroundColor = Color.black;
        }

        public void SetSlotType(Type t)
        {
            if (t != typeof(MaskSlot) && t != typeof(ColorTextureSlot))
            {
                throw new System.ArgumentException($"Invalid slot type, must be {typeof(MaskSlot).Name} or {typeof(ColorTextureSlot).Name}");
            }

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
            m_inputSlot = SlotProvider.Create(m_slotType, "Input", SlotDirection.Input, 0);
            m_outputSlot = SlotProvider.Create(m_slotType, "Output", SlotDirection.Output, 100);
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
            RenderTextureFormat format = slotType == typeof(MaskSlot) ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGB32;
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution, format);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);

            if (tilingMode == TilingMode.TileX || tilingMode == TilingMode.TileXY)
            {
                m_material.EnableKeyword(TILE_X);
            }
            else
            {
                m_material.DisableKeyword(TILE_X);
            }

            if (tilingMode == TilingMode.TileY || tilingMode == TilingMode.TileXY)
            {
                m_material.EnableKeyword(TILE_Y);
            }
            else
            {
                m_material.DisableKeyword(TILE_Y);
            }

            Matrix4x4 transformMatrix = Matrix4x4.TRS(
                new Vector3(m_offset.x / 100f, m_offset.y / 100f, 0),
                Quaternion.Euler(0, 0, m_rotation),
                new Vector3(m_scale.x / 100f, m_scale.y / 100f, 1f)).inverse;
            Matrix4x4 uvRemap = Matrix4x4.TRS(Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f);
            m_material.SetMatrix(TRANSFORM_MATRIX, uvRemap * transformMatrix);
            m_material.SetColor(BACKGROUND_COLOR, m_backgroundColor);

            Drawing.DrawQuad(targetRt, Drawing.unitQuad, UVS, m_material, PASS);
            Object.DestroyImmediate(m_material);
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
            if (!m_inputSlotData.Equals(default))
            {
                m_inputSlot = Serializer.Deserialize<ISlot>(m_inputSlotData);
            }
            else
            {
                m_inputSlot = null;
            }

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
            if (m_outputSlot != null)
            {
                return new ISlot[] { m_outputSlot };
            }
            else
            {
                return new ISlot[] { };
            }
        }

        public override ISlot GetSlot(int id)
        {
            if (m_inputSlot != null && m_inputSlot.id == id)
            {
                return m_inputSlot;
            }
            if (m_outputSlot != null && m_outputSlot.id == id)
            {
                return m_outputSlot;
            }
            return null;
        }
    }
}
#endif
