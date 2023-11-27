#if VISTA
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;

namespace Pinwheel.Vista.Graph
{
    [CreateAssetMenu(menuName = "Vista/Terrain Graph", order = -10000)]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.azv7z6jyswp7")]
    public class TerrainGraph : GraphAsset
    {
        public delegate void GraphExecutionHandler(TerrainGraph graph, TerrainGenerationConfigs data, string[] nodeIds);
        public static GraphExecutionHandler onBeforeGraphExecution;
        public static GraphExecutionHandler onAfterGraphExecution;

        public delegate void NodeExecutionHandler(TerrainGraph graph, TerrainGenerationConfigs data, INode node);
        public static NodeExecutionHandler onBeforeNodeExecution;
        public static NodeExecutionHandler onAfterNodeExecution;

        public delegate void FillArgumentsHandler(TerrainGraph graph, IDictionary<int, Args> args);

        [SerializeField]
        private TerrainGenerationConfigs m_debugConfigs;
        public TerrainGenerationConfigs debugConfigs
        {
            get
            {
                return m_debugConfigs;
            }
            set
            {
                m_debugConfigs = value;
            }
        }

        [SerializeField]
        private bool m_allowSplitExecution;
        public bool allowSplitExecution
        {
            get
            {
                return m_allowSplitExecution;
            }
            set
            {
                m_allowSplitExecution = value;
            }
        }

        public override void Reset()
        {
            base.Reset();
            m_debugConfigs = TerrainGenerationConfigs.Create();
            m_allowSplitExecution = true;
        }

        public override bool AcceptNodeType(Type t)
        {
            return typeof(ExecutableNodeBase).IsAssignableFrom(t);
        }

        private void FillArguments(TerrainGenerationConfigs configs, IDictionary<int, Args> args, FillArgumentsHandler fillArgumentCallback = null)
        {
            args.Add(Args.RESOLUTION, Args.Create(configs.resolution));
            args.Add(Args.WORLD_BOUNDS, Args.Create(new Vector4(configs.worldBounds.x, configs.worldBounds.y, configs.worldBounds.width, configs.worldBounds.height)));
            args.Add(Args.TERRAIN_HEIGHT, Args.Create(configs.terrainHeight));
            args.Add(Args.SEED, Args.Create(configs.seed));
            args.Add(Args.OUTPUT_TEMP_HEIGHT, Args.Create(configs.shouldOutputTempHeight));
            fillArgumentCallback?.Invoke(this, args);
        }

        public DataPool ExecuteImmediate(string[] nodeIds, TerrainGenerationConfigs configs, GraphInputContainer inputContainer = null, FillArgumentsHandler fillArgumentsCallback = null)
        {
            DataPool data = new DataPool();
            Dictionary<int, Args> arguments = new Dictionary<int, Args>();
            FillArguments(configs, arguments, fillArgumentsCallback);
            GraphContext context = new GraphContext(this, nodeIds, data, arguments);
            if (inputContainer != null)
            {
                inputContainer.Bind(ref context);
            }

            if (onBeforeGraphExecution != null)
            {
                onBeforeGraphExecution.Invoke(this, configs, nodeIds);
            }

            Queue<INode> executionSequence = context.GetExecutionSequence();

            while (executionSequence.Count > 0)
            {
                ExecutableNodeBase n = executionSequence.Peek() as ExecutableNodeBase;

                if (n.isBypassed)
                {
                    n.Bypass(context);
                }
                else
                {
                    if (onBeforeNodeExecution != null)
                    {
                        onBeforeNodeExecution.Invoke(this, configs, n);
                    }

                    n.ExecuteImmediate(context);

                    if (onAfterNodeExecution != null)
                    {
                        onAfterNodeExecution.Invoke(this, configs, n);
                    }
                }
                executionSequence.Dequeue();
            }

            if (onAfterGraphExecution != null)
            {
                onAfterGraphExecution.Invoke(this, configs, nodeIds);
            }

            return data;
        }

        public ExecutionHandle Execute(string[] nodeIds, TerrainGenerationConfigs configs, GraphInputContainer inputContainer = null, FillArgumentsHandler fillArgumentsCallback = null)
        {
            ExecutionHandle handle = ExecutionHandle.Create();
            Dictionary<int, Args> arguments = new Dictionary<int, Args>();
            FillArguments(configs, arguments, fillArgumentsCallback);
            GraphContext context = new GraphContext(this, nodeIds, handle.data, arguments, handle.progress);
            if (inputContainer != null)
            {
                inputContainer.Bind(ref context);
            }
            handle.coroutines.Add(CoroutineUtility.StartCoroutine(ExecuteProgressive(nodeIds, configs, context, handle)));
            return handle;
        }

        private IEnumerator ExecuteProgressive(string[] nodeIds, TerrainGenerationConfigs configs, GraphContext context, ExecutionHandle handle)
        {
            if (onBeforeGraphExecution != null)
            {
                onBeforeGraphExecution.Invoke(this, configs, nodeIds);
            }

            Queue<INode> executionSequence = context.GetExecutionSequence();
            int totalNodeCount = executionSequence.Count;

            while (executionSequence.Count > 0)
            {
                ExecutableNodeBase n = executionSequence.Peek() as ExecutableNodeBase;

                if (n.isBypassed)
                {
                    n.Bypass(context);
                }
                else
                {
                    if (onBeforeNodeExecution != null)
                    {
                        onBeforeNodeExecution.Invoke(this, configs, n);
                    }
                    if (n.shouldSplitExecution && m_allowSplitExecution)
                    {
                        CoroutineHandle c = CoroutineUtility.StartCoroutine(n.Execute(context));
                        handle.coroutines.Add(c);
                        yield return c.coroutine;
                    }
                    else
                    {
                        n.ExecuteImmediate(context);
                    }
                    if (onAfterNodeExecution != null)
                    {
                        onAfterNodeExecution.Invoke(this, configs, n);
                    }
                }
                executionSequence.Dequeue();

                handle.progress.totalProgress = 1f - executionSequence.Count * 1.0f / totalNodeCount;
            }

            if (onAfterGraphExecution != null)
            {
                onAfterGraphExecution.Invoke(this, configs, nodeIds);
            }

            yield return null;
            handle.Complete();
        }

        public override IEnumerable<GraphAsset> GetDependencySubGraphs()
        {
            List<GraphAsset> dependencyGraphs = new List<GraphAsset>();
            List<TerrainSubGraphNode> subGraphNode = GetNodes<TerrainSubGraphNode>().ConvertAll(n => n as TerrainSubGraphNode);
            foreach (TerrainSubGraphNode n in subGraphNode)
            {
                if (n.graph != null)
                {
                    dependencyGraphs.Add(n.graph);
                }
            }
            return dependencyGraphs;
        }

        public override bool Validate()
        {
            bool subgraphChanged = false;
            foreach (INode n in m_nodes)
            {
                if (n is TerrainSubGraphNode sgn)
                {
                    subgraphChanged |= sgn.UpdateSlotsArray();
                }
            }

            bool selfChanged = base.Validate();
            return subgraphChanged || selfChanged;
        }
    }
}
#endif
