#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Select Position",
        path = "General/Select Position",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.msjsbjgceuev",
        keywords = "",
        description = "Manually select a set of points.")]
    public class SelectPositionNode : ExecutableNodeBase
    {
        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        protected Vector2[] m_positions;
        public Vector2[] positions
        {
            get
            {
                if (m_positions == null || m_positions.Length == 0)
                {
                    return new Vector2[0];
                }
                else
                {
                    Vector2[] positionCopied = new Vector2[m_positions.Length];
                    System.Array.Copy(m_positions, positionCopied, m_positions.Length);
                    return positionCopied;
                }
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    m_positions = new Vector2[0];
                }
                else
                {
                    m_positions = new Vector2[value.Length];
                    System.Array.Copy(value, m_positions, value.Length);
                }
            }
        }

        public SelectPositionNode() : base()
        {
            m_positions = new Vector2[0];
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int instanceCount = Mathf.Max(8, 8 * Mathf.CeilToInt(m_positions.Length * 1.0f / 8.0f));
            PositionSample[] positionSamples = new PositionSample[instanceCount];
            for (int i = 0; i < m_positions.Length; ++i)
            {
                positionSamples[i] = new PositionSample()
                {
                    isValid = 1,
                    position = new Vector3(m_positions[i].x, 0, m_positions[i].y)
                };
            }

            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor outputDesc = DataPool.BufferDescriptor.Create(instanceCount * PositionSample.SIZE);
            ComputeBuffer outputBuffer = context.CreateBuffer(outputDesc, outputRef);
            outputBuffer.SetData(positionSamples);
        }
    }
}
#endif
