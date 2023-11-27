#if VISTA
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public struct GraphContext
    {
        private GraphAsset m_graph;
        private GraphAsset graph
        {
            get
            {
                return m_graph;
            }
        }

        internal DataPool m_dataPool;

        private List<string> m_nodeIds;
        private Dictionary<SlotRef, SlotRef> m_inputLinks;
        private Queue<INode> m_executionSequence;
        private Dictionary<string, int> m_refCount;
        private Dictionary<string, SlotRef> m_variables; //set a name for a slot
        private Dictionary<SlotRef, string> m_variableLinks; //many slots can point to a named slot (var slot)

        private Dictionary<SlotRef, string> m_externalBindings; //is a slot point to an internal (data pool) or external (graph inputs) resources
        private Dictionary<string, RenderTexture> m_externalTextures;
        private Dictionary<string, ComputeBuffer> m_externalBuffers;

        private Dictionary<int, Args> m_arguments;
        private ExecutionProgress m_progress;

        public GraphContext(GraphAsset graph, string[] nodeIds, DataPool pool = null, IDictionary<int, Args> args = null, ExecutionProgress progress = null)
        {
            m_graph = graph;
            m_inputLinks = new Dictionary<SlotRef, SlotRef>();
            m_refCount = new Dictionary<string, int>();
            m_executionSequence = new Queue<INode>();
            m_nodeIds = nodeIds.Distinct().ToList();
            m_variables = new Dictionary<string, SlotRef>();
            m_variableLinks = new Dictionary<SlotRef, string>();
            m_externalBindings = new Dictionary<SlotRef, string>();
            m_externalTextures = new Dictionary<string, RenderTexture>();
            m_externalBuffers = new Dictionary<string, ComputeBuffer>();
            m_dataPool = pool;
            if (args != null)
            {
                m_arguments = new Dictionary<int, Args>(args);
            }
            else
            {
                m_arguments = new Dictionary<int, Args>();
            }
            m_progress = progress;

            CheckRecursiveGraph();
            BuildInputLinks();
            BuildExecutionSequence();
            RemoveRedundantInputLinks();
            BuildReferenceCount();
        }

        private void CheckRecursiveGraph()
        {
            Stack<GraphAsset> graphTrace = new Stack<GraphAsset>();
            Stack<bool> flags = new Stack<bool>();
            graphTrace.Push(graph);
            flags.Push(false);

            while (graphTrace.Count > 0)
            {
                GraphAsset g = graphTrace.Peek();
                bool f = flags.Peek();
                if (f == true)
                {
                    graphTrace.Pop();
                    flags.Pop();
                }
                else
                {
                    flags.Pop();
                    flags.Push(true);

                    IEnumerable<GraphAsset> dependency = g.GetDependencySubGraphs().Distinct();
                    foreach (GraphAsset d in dependency)
                    {
                        if (graphTrace.Contains(d))
                        {
                            graphTrace.Push(d);
                            string sequence = StackToString<GraphAsset>(graphTrace, (g0) => { return g0.name; }, "->");
                            string errorMessage = $"Cannot resolve graph execution sequence, there is recursive sub-graph reference(s): {sequence}";
                            throw new RecursiveGraphReferenceException(errorMessage);
                        }
                        graphTrace.Push(d);
                        flags.Push(false);
                    }
                }
            }
        }

        private string StackToString<T>(Stack<T> stack, System.Func<T, string> toStringFunc, string separator)
        {
            Stack<T> st = new Stack<T>(stack);
            StringBuilder sb = new StringBuilder();
            while (st.Count > 0)
            {
                T o = st.Pop();
                if (toStringFunc != null)
                {
                    sb.Append(toStringFunc(o));
                }
                else
                {
                    sb.Append(o.ToString());
                }
                sb.Append("~");
            }
            string s = sb.ToString();
            return s.Trim('~').Replace("~", separator);
        }

        private void BuildInputLinks()
        {
            List<IEdge> edges = m_graph.m_edges;
            foreach (IEdge e in edges)
            {
                m_inputLinks[e.inputSlot] = e.outputSlot;
            }
        }

        private void RemoveRedundantInputLinks()
        {
            List<string> ids = new List<string>();
            foreach (INode n in m_executionSequence)
            {
                ids.Add(n.id);
            }

            List<SlotRef> toRemove = new List<SlotRef>();
            List<IEdge> edges = m_graph.m_edges;
            foreach (IEdge e in edges)
            {
                if (!ids.Contains(e.inputSlot.nodeId))
                {
                    toRemove.Add(e.inputSlot);
                }
            }

            foreach (SlotRef s in toRemove)
            {
                m_inputLinks.Remove(s);
            }
        }

        public void LinkToVariable(SlotRef slotRef, string varName)
        {
            if (string.IsNullOrEmpty(varName))
            {
                throw new System.ArgumentException("varName must not be null or empty");
            }
            m_variableLinks[slotRef] = varName;
        }

        public void LinkToInvalid(SlotRef slotRef)
        {
            m_variableLinks[slotRef] = "";
        }

        public SlotRef GetInputLink(SlotRef inputSlot)
        {
            SlotRef s;
            if (m_inputLinks.TryGetValue(inputSlot, out s))
            {
                if (m_variableLinks.ContainsKey(s))
                {
                    string varName = m_variableLinks[s];
                    return GetVariable(varName);
                }
                else
                {
                    return s;
                }
            }
            else
            {
                return SlotRef.invalid;
            }
        }

        public SlotRef GetInputLink(string nodeId, int slotId)
        {
            return GetInputLink(new SlotRef(nodeId, slotId));
        }

        private void BuildExecutionSequence()
        {
            Stack<INode>[] stacks = new Stack<INode>[m_nodeIds.Count];
            for (int i = 0; i < m_nodeIds.Count; ++i)
            {
                INode node = graph.GetNode(m_nodeIds[i]);
                if (node == null)
                {
                    throw new System.ArgumentException($"Cannot execute a null node with id {m_nodeIds[i]}");
                }
                stacks[i] = CreateExecutionStack(node);
            }

            m_executionSequence = new Queue<INode>();
            while (HasItem(stacks))
            {
                for (int i = 0; i < stacks.Length; ++i)
                {
                    if (stacks[i].Count > 0)
                    {
                        INode n = stacks[i].Pop();
                        if (!m_executionSequence.Contains(n))
                        {
                            m_executionSequence.Enqueue(n);
                        }
                    }
                }
            }
        }

        private bool HasItem<T>(Stack<T>[] stacks)
        {
            for (int i = 0; i < stacks.Length; ++i)
            {
                if (stacks[i].Count > 0)
                    return true;
            }
            return false;
        }

        private Stack<INode> CreateExecutionStack(INode node)
        {
            Stack<INode> nodeTrace = new Stack<INode>();
            Stack<INode> result = new Stack<INode>();
            nodeTrace.Push(node);
            while (nodeTrace.Count > 0)
            {
                INode n = nodeTrace.Pop();
                if (result.Count > 1 && n == node)
                {
                    throw new System.Exception($"Unable to resolve execution stack. Make sure there is no loop in the connection, especially Get/Set Variable nodes.");
                }
                result.Push(n);

                ISlot[] inputSlots = n.GetInputSlots();
                foreach (ISlot inputSlot in inputSlots)
                {
                    SlotRef inputRef = new SlotRef(n.id, inputSlot.id);
                    SlotRef outputRef = GetInputLink(inputRef);
                    if (outputRef.Equals(SlotRef.invalid))
                        continue;

                    INode connectedNode = graph.GetNode(outputRef.nodeId);
                    nodeTrace.Push(connectedNode);
                }

                if (n is IHasDependencyNodes hdn)
                {
                    IEnumerable<INode> dependencies = hdn.GetDependencies(graph.m_nodes);
                    if (dependencies != null)
                    {
                        foreach (INode d in dependencies)
                        {
                            nodeTrace.Push(d);
                        }
                    }
                }
            }
            return result;
        }

        public Queue<INode> GetExecutionSequence()
        {
            return new Queue<INode>(m_executionSequence);
        }

        private void BuildReferenceCount()
        {
            foreach (SlotRef s in m_inputLinks.Values)
            {
                string name = DataPool.GetName(s.nodeId, s.slotId);
                if (m_refCount.ContainsKey(name))
                {
                    m_refCount[name] += 1;
                }
                else
                {
                    m_refCount.Add(name, 1);
                }
            }

            foreach (string id in m_nodeIds)
            {
                INode node = graph.GetNode(id);
                ISlot[] outputs = node.GetOutputSlots();
                foreach (ISlot s in outputs)
                {
                    string name = DataPool.GetName(node.id, s.id);
                    m_refCount[name] = int.MaxValue;
                }
            }

            if (m_dataPool != null)
            {
                m_dataPool.SetReferenceCount(m_refCount);
            }
        }

        public Dictionary<string, int> GetInitialReferenceCount()
        {
            return m_refCount;
        }

        public Args GetArg(int id)
        {
            Args args;
            if (m_arguments != null && m_arguments.TryGetValue(id, out args))
            {
                return args;
            }
            else
            {
                return default;
            }
        }

        public Dictionary<int, Args> GetArgs()
        {
            return m_arguments;
        }

        public RenderTexture CreateRenderTarget(DataPool.RtDescriptor desc, SlotRef slotRef)
        {
            return m_dataPool.CreateRenderTarget(desc, slotRef);
        }

        public ComputeBuffer CreateBuffer(DataPool.BufferDescriptor desc, SlotRef slotRef)
        {
            return m_dataPool.CreateBuffer(desc, slotRef).buffer;
        }

        public RenderTexture CreateTemporaryRT(DataPool.RtDescriptor desc, string uniqueName)
        {
            return m_dataPool.CreateTemporaryRT(desc, uniqueName);
        }

        public ComputeBuffer CreateTemporaryBuffer(DataPool.BufferDescriptor desc, string uniqueName)
        {
            return m_dataPool.CreateTemporaryBuffer(desc, uniqueName).buffer;
        }

        public RenderTexture GetTexture(SlotRef slotRef)
        {
            bool isExternal = m_externalBindings.ContainsKey(slotRef);
            if (isExternal)
            {
                string name = m_externalBindings[slotRef];
                RenderTexture t;
                if (m_externalTextures.TryGetValue(name, out t))
                {
                    return t;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return m_dataPool.GetRT(slotRef);
            }
        }

        public ComputeBuffer GetBuffer(SlotRef slotRef)
        {
            bool isExternal = m_externalBindings.ContainsKey(slotRef);
            if (isExternal)
            {
                string name = m_externalBindings[slotRef];
                ComputeBuffer buffer;
                if (m_externalBuffers.TryGetValue(name, out buffer))
                {
                    return buffer;
                }
                return null;
            }
            else
            {
                GraphBuffer b = m_dataPool.GetBuffer(slotRef);
                if (b != null)
                {
                    return b.buffer;
                }
                else
                {
                    return null;
                }
            }
        }

        public void ReleaseTemporary(string name)
        {
            m_dataPool.ReleaseReference(name);
        }

        public void ReleaseReference(SlotRef slotRef)
        {
            m_dataPool.ReleaseReference(slotRef);
        }

        public int GetReferenceCount(SlotRef slotRef)
        {
            return m_dataPool.GetUsageCount(slotRef);
        }

        public bool IsTargetNode(string id)
        {
            return m_nodeIds.Contains(id);
        }

        public bool HasVariable(string varName)
        {
            return m_variables.ContainsKey(varName);
        }

        public void SetVariable(string varName, SlotRef slotRef)
        {
            if (string.IsNullOrEmpty(varName))
            {
                throw new System.ArgumentException("varName must not be null or empty");
            }
            if (m_variables.ContainsKey(varName))
            {
                throw new System.ArgumentException($"Variable {varName} is already exist");
            }
            m_variables.Add(varName, slotRef);
            m_dataPool.SetReferenceCount(slotRef, int.MaxValue);
        }

        public SlotRef GetVariable(string varName)
        {
            if (string.IsNullOrEmpty(varName))
            {
                return SlotRef.invalid;
            }
            SlotRef slotRef;
            if (m_variables.TryGetValue(varName, out slotRef))
            {
                return slotRef;
            }
            return SlotRef.invalid;
        }

        public void SetExternal(SlotRef slotRef, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException("External resource name must not be null or empty");
            }
            m_externalBindings[slotRef] = name;
        }

        public void AddExternalTexture(string name, RenderTexture texture)
        {
            m_externalTextures.Add(name, texture);
        }

        public void AddExternalBuffer(string name, ComputeBuffer buffer)
        {
            m_externalBuffers.Add(name, buffer);
        }

        internal void BindTexture(SlotRef slotRef, GraphRenderTexture texture)
        {
            m_dataPool.BindTexture(slotRef, texture);
        }

        internal void BindBuffer(SlotRef slotRef, GraphBuffer buffer)
        {
            m_dataPool.BindBuffer(slotRef, buffer);
        }

        public void SetCurrentProgress(float p)
        {
            if (m_progress != null)
            {
                m_progress.currentProgress = p;
            }
        }
    }
}
#endif
