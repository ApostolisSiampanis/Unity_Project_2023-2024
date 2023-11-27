#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Spread",
        path = "General/Spread",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.fe0r5cyj1g54",
        keywords = "",
        description = "Spread the source positions into many others. Use this node to have a more realistic tree spawning.")]
    public class SpreadNode : ExecutableNodeBase, IHasSeed
    {
        public readonly BufferSlot inputPositionSlot = new BufferSlot("Positions", SlotDirection.Input, 0);
        public readonly MaskSlot distanceSlot = new MaskSlot("Distance", SlotDirection.Input, 1);
        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private int m_count;
        public int count
        {
            get
            {
                return m_count;
            }
            set
            {
                m_count = Mathf.Max(0, Utilities.MultipleOf8(Mathf.Clamp(value, 0, 15)) - 1);
            }
        }

        [SerializeField]
        private float m_distance;
        public float distance
        {
            get
            {
                return m_distance;
            }
            set
            {
                m_distance = Mathf.Clamp01(value);
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

        [SerializeField]
        private bool m_keepSourcePoints;
        public bool keepSourcePoints
        {
            get
            {
                return m_keepSourcePoints;
            }
            set
            {
                m_keepSourcePoints = value;
            }
        }

        private readonly string SHADER_NAME = "Vista/Shaders/Graph/Spread";
        private readonly int SRC_BUFFER = Shader.PropertyToID("_SrcBuffer");
        private readonly int DEST_BUFFER = Shader.PropertyToID("_DestBuffer");
        private readonly int DISTANCE_MAP = Shader.PropertyToID("_DistanceMap");
        private readonly int DISTANCE = Shader.PropertyToID("_Distance");
        private readonly int COUNT = Shader.PropertyToID("_Count");
        private readonly int SEED = Shader.PropertyToID("_Seed");
        private readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
        private readonly int KERNEL = 0;

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private static readonly string KW_KEEP_SOURCE_POINTS = "KEEP_SOURCE_POINTS";
        private static readonly string KW_HAS_DISTANCE_MAP = "HAS_DISTANCE_MAP";

        private ComputeShader m_shader;

        public SpreadNode() : base()
        {
            m_count = 7;
            m_distance = 0.05f;
            m_seed = 0;
            m_keepSourcePoints = true;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            SlotRef inputPositionRefLink = context.GetInputLink(m_id, inputPositionSlot.id);
            ComputeBuffer inputBuffer = context.GetBuffer(inputPositionRefLink);
            if (inputBuffer == null)
            {
                return;
            }
            if (inputBuffer.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse buffer {inputPositionSlot.name}, node id {m_id}");
                return;
            }

            SlotRef distanceMapRefLink = context.GetInputLink(m_id, distanceSlot.id);
            Texture distanceMap = context.GetTexture(distanceMapRefLink);

            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(inputBuffer.count * (m_count + 1));
            ComputeBuffer outputBuffer = context.CreateBuffer(desc, outputRef);

            m_shader = Resources.Load<ComputeShader>(SHADER_NAME);
            m_shader.SetBuffer(KERNEL, SRC_BUFFER, inputBuffer);
            m_shader.SetBuffer(KERNEL, DEST_BUFFER, outputBuffer);
            m_shader.SetFloat(DISTANCE, m_distance);
            if (distanceMap != null)
            {
                m_shader.SetTexture(KERNEL, DISTANCE_MAP, distanceMap);
                m_shader.EnableKeyword(KW_HAS_DISTANCE_MAP);
            }
            else
            {
                m_shader.DisableKeyword(KW_HAS_DISTANCE_MAP);
            }
            if (keepSourcePoints)
            {
                m_shader.EnableKeyword(KW_KEEP_SOURCE_POINTS);
            }
            else
            {
                m_shader.DisableKeyword(KW_KEEP_SOURCE_POINTS);
            }
            m_shader.SetInt(COUNT, m_count);

            int srcInstanceCount = inputBuffer.count / PositionSample.SIZE;

            int baseSeed = context.GetArg(Args.SEED).intValue;
            System.Random rnd = new System.Random(m_seed ^ baseSeed);
            Vector4 randomSeed = new Vector4((float)(10 * rnd.NextDouble()), (float)(-10 * rnd.NextDouble()), (float)(20 * rnd.NextDouble()), (float)(-20 * rnd.NextDouble()));
            m_shader.SetVector(SEED, randomSeed);

            int totalThreadGroupX = (srcInstanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                m_shader.SetInt(BASE_INDEX, baseIndex);
                m_shader.Dispatch(KERNEL, threadGroupX, 1, 1);
            }

            context.ReleaseReference(inputPositionRefLink);
            context.ReleaseReference(distanceMapRefLink);
            Resources.UnloadAsset(m_shader);
        }
    }
}
#endif
