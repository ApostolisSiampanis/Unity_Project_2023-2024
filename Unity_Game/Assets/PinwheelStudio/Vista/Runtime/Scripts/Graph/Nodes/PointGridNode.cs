#if VISTA
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Point Grid",
        path = "Legacy/Point Grid",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.kxq5ap6gx0av",
        keywords = "",
        description = "Generate a set of point samples in a uniform grid manner. Use in conjunction with Offset and Spread node to have a randomly looking points set.")]
    public class PointGridNode : ExecutableNodeBase
    {
        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private int m_density;
        public int density
        {
            get
            {
                return m_density;
            }
            set
            {
                m_density = Utilities.MultipleOf8(Mathf.Clamp(value, 8, 10000));
            }
        }

        public enum RelativeTo
        {
            AbsoluteBiomeSize,
            Per100Meters,
            Per1000Meters
        }

        [SerializeField]
        private RelativeTo m_relativeTo;
        public RelativeTo relativeTo
        {
            get
            {
                return m_relativeTo;
            }
            set
            {
                m_relativeTo = value;
            }
        }

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/PointGrid";
        private static readonly int POSITIONS = Shader.PropertyToID("_Positions");
        private static readonly int GRID_BOUNDS = Shader.PropertyToID("_GridBounds");
        private static readonly int BIOME_BOUNDS = Shader.PropertyToID("_BiomeBounds");
        private static readonly int DENSITY = Shader.PropertyToID("_Density");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
        private static readonly int KERNEL_INDEX = 0;

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private ComputeShader m_computeShader;

        public PointGridNode() : base()
        {
            m_density = 8;
            m_relativeTo = RelativeTo.Per100Meters;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            int gridDimension;
            ComputeShader shader = SetupComputeShader(context, out gridDimension);

            int instanceCount = gridDimension * gridDimension;
            int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                shader.SetInt(BASE_INDEX, baseIndex);
                shader.Dispatch(KERNEL_INDEX, threadGroupX, 1, 1);
                yield return null;
            }
            CleanUp();
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int gridDimension;
            ComputeShader shader = SetupComputeShader(context, out gridDimension);

            int instanceCount = gridDimension * gridDimension;
            int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                shader.SetInt(BASE_INDEX, baseIndex);
                shader.Dispatch(KERNEL_INDEX, threadGroupX, 1, 1);
            }
            CleanUp();
        }

        private ComputeShader SetupComputeShader(GraphContext context, out int gridDimension)
        {
            Vector4 biomeBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            Vector4 gridBounds = biomeBounds;
            gridDimension = m_density;
            if (m_relativeTo != RelativeTo.AbsoluteBiomeSize)
            {
                float n = m_relativeTo == RelativeTo.Per100Meters ? 100f : 1000f;

                gridBounds.z = Mathf.Ceil(biomeBounds.z / n) * n;
                gridBounds.w = Mathf.Ceil(biomeBounds.w / n) * n;
                gridDimension = (int)(gridBounds.z / n) * m_density;
            }

            int instanceCount = gridDimension * gridDimension;
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(instanceCount * PositionSample.SIZE);
            ComputeBuffer positionOutputBuffer = context.CreateBuffer(desc, outputRef);

            m_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            m_computeShader.SetBuffer(KERNEL_INDEX, POSITIONS, positionOutputBuffer);
            m_computeShader.SetVector(BIOME_BOUNDS, biomeBounds);
            m_computeShader.SetVector(GRID_BOUNDS, gridBounds);
            m_computeShader.SetInt(DENSITY, gridDimension);

            return m_computeShader;
        }

        private void CleanUp()
        {
            Resources.UnloadAsset(m_computeShader);
        }
    }
}
#endif
