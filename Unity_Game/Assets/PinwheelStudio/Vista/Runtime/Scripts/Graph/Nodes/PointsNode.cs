#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using Pinwheel.Vista.Graph;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Points",
        path = "General/Points",
        description = "Generate a grid of points with desired spacing in meters. Use in conjunction with Offset and Spread node to have a randomly looking points set.")]
    public class PointsNode : ExecutableNodeBase
    {
        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private Vector2 m_spacing;
        public Vector2 spacing
        {
            get
            {
                return m_spacing;
            }
            set
            {
                float x = Mathf.Max(1, value.x);
                float y = Mathf.Max(1, value.y);
                m_spacing.Set(x, y);
            }
        }

        private static readonly string SHADER_NAME = "Vista/Shaders/Graph/Points";
        private static readonly int POSITIONS_BUFFER = Shader.PropertyToID("_Positions");
        private static readonly int LOWER_LEFT_POINT = Shader.PropertyToID("_LowerLeftPoint");
        private static readonly int SPACING = Shader.PropertyToID("_Spacing");
        private static readonly int BIOME_BOUNDS = Shader.PropertyToID("_BiomeBounds");
        private static readonly int GRID_DIMENSION = Shader.PropertyToID("_GridDimension");

        private static readonly int KERNEL_INDEX = 0;

        private ComputeShader m_computeShader;

        public PointsNode() : base()
        {
            m_spacing = new Vector2(100, 100);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            Vector4 biomeBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float llx = Mathf.Ceil(biomeBounds.x / m_spacing.x) * m_spacing.x;
            float lly = Mathf.Ceil(biomeBounds.y / m_spacing.y) * m_spacing.y;
            int gridDimX = Utilities.MultipleOf8(Mathf.CeilToInt(biomeBounds.z / m_spacing.x) + 1);
            int gridDimY = Utilities.MultipleOf8(Mathf.CeilToInt(biomeBounds.w / m_spacing.y) + 1);

            int instanceCount = gridDimX * gridDimY;
            if (instanceCount == 0)
                return;

            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(instanceCount * PositionSample.SIZE);
            ComputeBuffer positionOutputBuffer = context.CreateBuffer(desc, outputRef);

            m_computeShader = Resources.Load<ComputeShader>(SHADER_NAME);
            m_computeShader.SetBuffer(KERNEL_INDEX, POSITIONS_BUFFER, positionOutputBuffer);
            m_computeShader.SetVector(LOWER_LEFT_POINT, new Vector4(llx, lly));
            m_computeShader.SetVector(SPACING, m_spacing);
            m_computeShader.SetVector(BIOME_BOUNDS, biomeBounds);
            m_computeShader.SetVector(GRID_DIMENSION, new Vector4(gridDimX, gridDimY));

            m_computeShader.Dispatch(KERNEL_INDEX, (gridDimX + 7) / 8, (gridDimY + 7) / 8, 1); //don't need multi-pass here because we use both group X and Y, once the group exceed 65k, the buffer size will over the limit anyway

            Resources.UnloadAsset(m_computeShader);
        }
    }
}
#endif
