#if VISTA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Sub Graph",
        path = "",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.n3ol7f92ufdu",
        keywords = "",
        description = "Execute a terrain graph and take out the result.\nThe input/output slots of this node is defined by the Input/Output nodes of the sub-graph.",
        hideFromDoc = true)]
    public class TerrainSubGraphNode : ExecutableNodeBase, IHasDynamicSlotCount, ISerializationCallbackReceiver, ISetupWithHint, IHasSeed
    {
        [System.NonSerialized]
        private ISlot[] m_inputSlots;
        public ISlot[] inputSlots
        {
            get
            {
                return m_inputSlots;
            }
        }

        [SerializeField]
        private List<Serializer.JsonObject> m_inputSlotsData;

        [System.NonSerialized]
        private ISlot[] m_outputSlots;
        public ISlot[] outputSlots
        {
            get
            {
                return m_outputSlots;
            }
        }

        [SerializeField]
        private List<Serializer.JsonObject> m_outputSlotsData;

        [SerializeAsset]
        private TerrainGraph m_graph;
        [NonExposable]
        public TerrainGraph graph
        {
            get
            {
                return m_graph;
            }
            set
            {
                TerrainGraph oldValue = m_graph;
                TerrainGraph newValue = value;
                m_graph = newValue;
                if (oldValue != newValue)
                {
                    UpdateSlotsArray();
                    if (slotsChanged != null)
                    {
                        slotsChanged.Invoke(this);
                    }
                }
            }
        }

        [SerializeField]
        private bool m_overrideSeed;
        public bool overrideSeed
        {
            get
            {
                return m_overrideSeed;
            }
            set
            {
                m_overrideSeed = value;
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

        public const int MAX_INPUT = 100;
        public const int MAX_OUTPUT = 100;

        public event IHasDynamicSlotCount.SlotsChangedHandler slotsChanged;

        internal delegate void GatherInputTextureHandler(TerrainSubGraphNode sender, string name, RenderTexture inputTexture);
        internal delegate void GatherInputBufferHandler(TerrainSubGraphNode sender, string name, ComputeBuffer inputBuffer);
        internal static GatherInputTextureHandler gatherInputTextureCallback;
        internal static GatherInputBufferHandler gatherInputBufferCallback;

        public TerrainSubGraphNode() : base()
        {
            UpdateSlotsArray();
        }

        public bool UpdateSlotsArray()
        {
            int inputId = 0;
            int outputId = MAX_INPUT + 0;
            List<ISlot> inputSlots = new List<ISlot>();
            List<ISlot> outputSlots = new List<ISlot>();
            if (m_graph != null)
            {
                List<InputNode> inputNodes = m_graph.GetNodes<InputNode>().ConvertAll(n => n as InputNode);
                foreach (InputNode n in inputNodes)
                {
                    ISlot s = SlotProvider.Create(n.slotType, n.inputName, SlotDirection.Input, inputId);
                    inputSlots.Add(s);
                    inputId += 1;
                }

                List<OutputNode> outputNodes = m_graph.GetNodes<OutputNode>().ConvertAll(n => n as OutputNode);
                foreach (OutputNode n in outputNodes)
                {
                    ISlot s = SlotProvider.Create(n.slotType, n.outputName, SlotDirection.Output, outputId);
                    outputSlots.Add(s);
                    outputId += 1;
                }
            }

            if (inputSlots.Count > MAX_INPUT || outputSlots.Count > MAX_OUTPUT)
            {
                throw new System.Exception("Too many input/output in the sub graph");
            }

            ISlot[] newInputSlots = inputSlots.ToArray();
            ISlot[] newOutputSlots = outputSlots.ToArray();
            bool ioChanged = false;
            if (!SlotUtilities.AreEqual(m_inputSlots, newInputSlots))
                ioChanged = true;
            else if (!SlotUtilities.AreEqual(m_outputSlots, newOutputSlots))
                ioChanged = true;

            m_inputSlots = inputSlots.ToArray();
            m_outputSlots = outputSlots.ToArray();
            return ioChanged;
        }

        private GraphInputContainer GetInputContainer(GraphContext context)
        {
            List<InputNode> inputNodes = m_graph.GetNodes<InputNode>().ConvertAll(n => n as InputNode);
            GraphInputContainer inputContainer = new GraphInputContainer();
            List<SlotRef> inputRefLinks = new List<SlotRef>();
            foreach (ISlot s in m_inputSlots)
            {
                InputNode node = inputNodes.Find(n => n.inputName.Equals(s.name));
                if (node == null)
                    continue;
                SlotRef inputLink = context.GetInputLink(m_id, s.id);
                inputRefLinks.Add(inputLink);
                if (node.slotType == typeof(BufferSlot))
                {
                    ComputeBuffer buffer = context.GetBuffer(inputLink);
                    if (buffer != null)
                    {
                        inputContainer.AddBuffer(node.inputName, buffer);
                        gatherInputBufferCallback?.Invoke(this, node.inputName, buffer);
                    }
                }
                else
                {
                    RenderTexture rt = context.GetTexture(inputLink);
                    if (rt != null)
                    {
                        inputContainer.AddTexture(node.inputName, rt);
                        gatherInputTextureCallback?.Invoke(this, node.inputName, rt);
                    }
                }
            }

            foreach (SlotRef s in inputRefLinks)
            {
                context.ReleaseReference(s);
            }


            gatherInputTextureCallback = null;
            gatherInputBufferCallback = null;

            return inputContainer;
        }

        private void BindOutput(GraphContext context, DataPool data, List<OutputNode> outputNodes)
        {
            foreach (ISlot s in m_outputSlots)
            {
                OutputNode node = outputNodes.Find(n => n.outputName.Equals(s.name));
                if (node == null)
                    continue;
                if (node.slotType == typeof(BufferSlot))
                {
                    GraphBuffer buffer = data.RemoveBufferFromPool(node.mainOutputSlot);
                    if (buffer != null)
                    {
                        SlotRef outputRef = new SlotRef(m_id, s.id);
                        context.BindBuffer(outputRef, buffer);
                    }
                }
                else
                {
                    GraphRenderTexture rt = data.RemoveRTFromPool(node.mainOutputSlot);
                    if (rt != null)
                    {
                        SlotRef outputRef = new SlotRef(m_id, s.id);
                        context.BindTexture(outputRef, rt);
                    }
                }
            }
        }

        private TerrainGenerationConfigs CreateConfigs(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            int baseSeed = overrideSeed ? m_seed : context.GetArg(Args.SEED).intValue;

            TerrainGenerationConfigs configs = new TerrainGenerationConfigs();
            configs.resolution = baseResolution;
            configs.worldBounds = new Rect(worldBounds.x, worldBounds.y, worldBounds.z, worldBounds.w);
            configs.terrainHeight = terrainHeight;
            configs.seed = baseSeed;
            return configs;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            if (m_graph == null)
                return;
            GraphInputContainer inputContainer = GetInputContainer(context);
            List<string> nodeIds = new List<string>();
            List<OutputNode> outputNodes = m_graph.GetNodesOfType<OutputNode>();
            foreach (OutputNode n in outputNodes)
            {
                nodeIds.Add(n.id);
            }

            DataPool data = m_graph.ExecuteImmediate(nodeIds.ToArray(), CreateConfigs(context), inputContainer);
            BindOutput(context, data, outputNodes);
            data.Dispose();
        }

        public override IEnumerator Execute(GraphContext context)
        {
            if (m_graph == null)
                yield break;
            GraphInputContainer inputContainer = GetInputContainer(context);
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            List<string> nodeIds = new List<string>();
            List<OutputNode> outputNodes = m_graph.GetNodesOfType<OutputNode>();
            foreach (OutputNode n in outputNodes)
            {
                nodeIds.Add(n.id);
            }

            ExecutionHandle handle = m_graph.Execute(nodeIds.ToArray(), CreateConfigs(context), inputContainer);
            yield return handle;

            BindOutput(context, handle.data, outputNodes);
            handle.Dispose();
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
            ISlot[] slots = id < MAX_INPUT ? m_inputSlots : m_outputSlots;
            for (int i = 0; i < slots.Length; ++i)
            {
                if (slots[i].id == id)
                    return slots[i];
            }
            return null;
        }

        public void OnBeforeSerialize()
        {
            if (m_inputSlots != null)
            {
                m_inputSlotsData = Serializer.Serialize<ISlot>(m_inputSlots);
            }
            else
            {
                m_inputSlotsData = new List<Serializer.JsonObject>();
            }

            if (m_outputSlots != null)
            {
                m_outputSlotsData = Serializer.Serialize<ISlot>(m_outputSlots);
            }
            else
            {
                m_outputSlotsData = new List<Serializer.JsonObject>();
            }
        }

        public void OnAfterDeserialize()
        {
            if (m_inputSlotsData != null)
            {
                m_inputSlots = Serializer.Deserialize<ISlot>(m_inputSlotsData).ToArray();
            }
            else
            {
                m_inputSlots = new ISlot[0];
            }

            if (m_outputSlotsData != null)
            {
                m_outputSlots = Serializer.Deserialize<ISlot>(m_outputSlotsData).ToArray();
            }
            else
            {
                m_outputSlots = new ISlot[0];
            }
        }

        public void SetupWithHint(string hint)
        {
#if UNITY_EDITOR
            TerrainGraph g = UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainGraph>(hint);
            graph = g;
#endif
        }
    }
}
#endif
