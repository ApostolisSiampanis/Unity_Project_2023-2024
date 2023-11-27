#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.VistaEditor.Graph
{
    public partial class TerrainGraphEditor : GraphEditorBase
    {
        internal IGraphAdapter m_adapter;

        internal TerrainGraphViewportContainer m_viewportContainer;

        internal ToolbarButton m_viewport2dButton;
        internal TerrainGraphViewport2d m_viewport2d;

        internal RatingBox m_ratingBox;

        internal ToolbarButton m_viewport3dButton;
        internal TerrainGraphViewport3d m_viewport3d;

        internal ExecutionHandle m_lastExecution;
        internal DataPool.MemoryStats m_currentMemStats;
        internal DataPool.MemoryStats m_peakMemStats;
        internal RenderTexture m_biomeMask;
        internal RenderTexture m_sceneHeightMap;
        internal RenderTexture[] m_textureInputsRT;

        internal bool m_needConstantUpdate2D;

        internal string m_display2dNodeId;
        internal string m_display3dHeightNodeId;
        internal string m_display3dMaskNodeId;
        internal string m_display3dPositionNodeId;
        internal List<string> m_display3dWeightNodeId;

        private const string KEY_VIEW_DATA = "viewData";

        [System.Serializable]
        private class ViewData
        {
            public string m_activeNodeId = string.Empty;
            public string m_display2dNodeId = string.Empty;
            public string m_display3dHeightNodeId = string.Empty;
            public string m_display3dMaskNodeId = string.Empty;
            public string m_display3dPositionNodeId = string.Empty;
            public List<string> m_display3dWeightNodeId = new List<string>();
        }

        public void Awake()
        {
            m_lastExecution = default;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SetupAdapter();
            EditorCoroutineUtility.StartCoroutine(OnEnableDelayed(), this);
        }

        private IEnumerator OnEnableDelayed()
        {
            yield return null;
            //PrepareEnvironment();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            SavePrefs();
            if (m_lastExecution != null)
            {
                m_lastExecution.Dispose();
            }
            //CleanUpEnvironment();
        }

        public void OnFocus()
        {
            EditorCoroutineUtility.StartCoroutine(OnFocusDelayed(), this);
        }

        private IEnumerator OnFocusDelayed()
        {
            yield return null;
            if (m_clonedGraph == null)
            {
                Close();
                yield break;
            }

            UpdateNodesVisual();
        }

        protected virtual void SetupAdapter()
        {
            TerrainGraphAdapter adapter = new TerrainGraphAdapter();
            adapter.Init(this);
            m_adapter = adapter;
        }

        private void SavePrefs()
        {
            if (clonedGraph == null)
                return;
            if (AssetDatabase.Contains(sourceGraph))
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceGraph));
                ViewData viewData = new ViewData();
                viewData.m_activeNodeId = m_activeNodeId;
                viewData.m_display2dNodeId = m_display2dNodeId;
                viewData.m_display3dHeightNodeId = m_display3dHeightNodeId;
                viewData.m_display3dMaskNodeId = m_display3dMaskNodeId;
                viewData.m_display3dPositionNodeId = m_display3dPositionNodeId;
                viewData.m_display3dWeightNodeId = m_display3dWeightNodeId;
                string json = EditorJsonUtility.ToJson(viewData);
                EditorPrefs.SetString(guid + KEY_VIEW_DATA, json);
            }
        }

        private void LoadPrefs()
        {
            if (clonedGraph == null)
                return;
            if (AssetDatabase.Contains(sourceGraph))
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceGraph));
                string json = EditorPrefs.GetString(guid + KEY_VIEW_DATA, "");
                ViewData viewData = new ViewData();
                try
                {
                    EditorJsonUtility.FromJsonOverwrite(json, viewData);
                }
                catch { }
                m_activeNodeId = viewData.m_activeNodeId;
                m_display2dNodeId = viewData.m_display2dNodeId;
                m_display3dHeightNodeId = viewData.m_display3dHeightNodeId;
                m_display3dMaskNodeId = viewData.m_display3dMaskNodeId;
                m_display3dPositionNodeId = viewData.m_display3dPositionNodeId;
                m_display3dWeightNodeId = viewData.m_display3dWeightNodeId;
            }
        }

        protected override void OnSetupGUI()
        {
            m_viewportContainer = new TerrainGraphViewportContainer() { name = "viewport-container" };
            m_viewportContainer.OnEnable();
            m_leftContainer.Insert(0, m_viewportContainer);

            m_viewport2dButton = new ToolbarButton() { text = "2D" };
            m_viewport2dButton.AddToClassList("active");
            m_viewport2dButton.clicked += OnDebug2dButtonClicked;
            m_toolbar.rightContainer.Add(m_viewport2dButton);
            m_toolbar.rightContainer.Add(new VerticalSeparator());

            m_viewport3dButton = new ToolbarButton() { text = "3D" };
            m_viewport3dButton.AddToClassList("active");
            m_viewport3dButton.clicked += OnDebug3dButtonClicked;
            m_toolbar.rightContainer.Add(m_viewport3dButton);
            m_toolbar.rightContainer.Add(new VerticalSeparator());

            m_viewport3d = new TerrainGraphViewport3d(this);
            m_viewportContainer.Add(m_viewport3d);
            m_viewport3d.OnEnable();

            m_viewport2d = new TerrainGraphViewport2d(this);
            m_viewportContainer.Add(m_viewport2d);
            m_viewport2d.OnEnable();

            Resizer viewport2dResizer = new Resizer(Resizer.Position.Left);
            viewport2dResizer.AttachTo(m_viewport2d);

            Resizer viewportContainerResizer = new Resizer(Resizer.Position.Bottom);
            viewportContainerResizer.AttachTo(m_viewportContainer);

            if (RatingBox.IsAppropriateForShowing())
            {
                m_ratingBox = new RatingBox();
                m_rightContainer.Add(m_ratingBox);
            }
        }

        protected override void OnTearDownGUI()
        {
            m_viewportContainer.OnDisable();
            m_viewport2d.OnDisable();
            m_viewport3d.OnDisable();
        }

        protected override void OnRegisterCallbacks()
        {
            m_graphView.nodeAdded += OnNodeAdded;
            TerrainGraph.onBeforeNodeExecution += OnBeforeNodeExecution;
            TerrainGraph.onAfterNodeExecution += OnAfterNodeExecution;
            TerrainGraph.onBeforeGraphExecution += OnBeforeGraphExecution;
            TerrainGraph.onAfterGraphExecution += OnAfterGraphExecution;
        }

        protected override void OnUnregisterCallbacks()
        {
            m_graphView.nodeAdded -= OnNodeAdded;
            TerrainGraph.onBeforeNodeExecution -= OnBeforeNodeExecution;
            TerrainGraph.onAfterNodeExecution -= OnAfterNodeExecution;
            TerrainGraph.onBeforeGraphExecution -= OnBeforeGraphExecution;
            TerrainGraph.onAfterGraphExecution -= OnAfterGraphExecution;
        }

        protected override void OnActiveNodeChanged(NodeView nv)
        {
            base.OnActiveNodeChanged(nv);
            ExecuteGraph();

            if (m_viewport2d != null && m_viewport2d.enabledInHierarchy)
            {
                m_viewport2d.RenderViewport();
                m_viewport2d.MarkDirtyRepaint();
            }
            if (m_viewport3d != null && m_viewport3d.enabledInHierarchy)
            {
                m_viewport3d.RenderViewport();
                m_viewport3d.MarkDirtyRepaint();
            }
            if (nv != null)
            {
                UpdateNodeVisual(nv);
            }
        }

        private void OnNodeAdded(NodeView nv)
        {
            UpdateNodeVisual(nv);
        }

        internal override void ExecuteGraph()
        {
            if (m_lastExecution != null)
            {
                m_lastExecution.Dispose();
            }

            EditorSettings editorSettings = EditorSettings.Get();
            if (editorSettings.troubleshootingSettings.dontExecuteGraphOnSelection)
            {
                return;
            }

            List<string> ids = new List<string>();
            if (clonedGraph.HasNode(m_activeNodeId))
            {
                ids.Add(m_activeNodeId);
            }
            if (clonedGraph.HasNode(m_display2dNodeId))
            {
                ids.Add(m_display2dNodeId);
            }
            if (clonedGraph.HasNode(m_display3dHeightNodeId))
            {
                ids.Add(m_display3dHeightNodeId);
            }
            if (clonedGraph.HasNode(m_display3dMaskNodeId))
            {
                ids.Add(m_display3dMaskNodeId);
            }
            if (clonedGraph.HasNode(m_display3dPositionNodeId))
            {
                ids.Add(m_display3dPositionNodeId);
            }

            if (ids.Count > 0)
            {
                GraphInputContainer inputContainer = new GraphInputContainer();
                TerrainGraph.FillArgumentsHandler fillArgumentsCallback = null;
                if (m_externalInputProvider != null)
                {
                    try
                    {
                        m_externalInputProvider.SetInput(inputContainer);
                        if (m_externalInputProvider is ITerrainGraphArgumentsFiller tgaf)
                        {
                            fillArgumentsCallback = tgaf.FillTerrainGraphArguments;
                        }
                    }
                    catch
                    {
                        Debug.LogWarning($"Failed to set external input for the graph {sourceGraph.name}. Please try closing and reopening the graph editor.");
                    }
                }

                TerrainGraph tg = clonedGraph as TerrainGraph;
                m_lastExecution = tg.Execute(ids.ToArray(), tg.debugConfigs, inputContainer, fillArgumentsCallback);
            }
        }

        internal DataPool ExecuteGraphImmediateAt(params string[] nodeIds)
        {
            if (m_lastExecution != null)
            {
                m_lastExecution.Dispose();
            }

            if (nodeIds.Length > 0)
            {
                GraphInputContainer inputContainer = new GraphInputContainer();
                TerrainGraph.FillArgumentsHandler fillArgumentsCallback = null;
                if (m_externalInputProvider != null)
                {
                    m_externalInputProvider.SetInput(inputContainer);
                    if (m_externalInputProvider is ITerrainGraphArgumentsFiller tgaf)
                    {
                        fillArgumentsCallback = tgaf.FillTerrainGraphArguments;
                    }
                }

                TerrainGraph tg = clonedGraph as TerrainGraph;
                DataPool data = tg.ExecuteImmediate(nodeIds, tg.debugConfigs, inputContainer, fillArgumentsCallback);
                return data;
            }
            else
            {
                return null;
            }
        }

        private void OnBeforeNodeExecution(GraphAsset graph, TerrainGenerationConfigs data, INode node)
        {
            if (graph != clonedGraph)
                return;

            m_graphView.Query<NodeView>().ForEach(nv =>
            {
                string id = nv.nodeId;
                if (id.Equals(node.id))
                {
                    nv.SetHighlight(true);
                }
            });
        }

        private void OnAfterNodeExecution(GraphAsset graph, TerrainGenerationConfigs data, INode node)
        {
            if (graph != clonedGraph)
                return;
        }

        private void OnBeforeGraphExecution(GraphAsset graph, TerrainGenerationConfigs data, string[] ids)
        {
            if (graph != clonedGraph)
                return;

            m_graphView.Query<NodeView>().ForEach(nv =>
            {
                nv.SetHighlight(false);
            });

            m_statusBar.SetProgressVisible(true);
            m_statusBar.SetProgress(0, 0);
        }

        private void OnAfterGraphExecution(GraphAsset graph, TerrainGenerationConfigs data, string[] ids)
        {
            if (graph != clonedGraph)
                return;
            if (m_viewport2d != null && m_viewport2d.enabledInHierarchy)
            {
                m_viewport2d.RenderViewport();
                m_viewport2d.MarkDirtyRepaint();
            }
            if (m_viewport3d != null && m_viewport3d.enabledInHierarchy)
            {
                m_viewport3d.RenderViewport();
                m_viewport3d.MarkDirtyRepaint();
            }
            if (m_lastExecution != null)
            {
                m_peakMemStats = m_lastExecution.data.GetMemoryStats();
                m_lastExecution.data.DisposeUnused();
                m_currentMemStats = m_lastExecution.data.GetMemoryStats();
            }

            m_statusBar.SetProgressVisible(false);
            if (m_externalInputProvider != null)
            {
                m_externalInputProvider.CleanUp();
            }
        }

        internal override void OnSaveRequest()
        {
            m_adapter.commandHandler.Save();
            foreach (GraphEditorBase editor in GraphEditorBase.openingEditors)
            {
                if (editor is TerrainGraphEditor graphEditor)
                {
                    graphEditor.ValidateGraph();
                }
            }
        }

        internal override void OnSaveAsRequest()
        {
            m_adapter.commandHandler.SaveAs();
        }

        internal override ISearcherProvider GetSearcherProvider()
        {
            return m_adapter.searcherProvider;
        }

        internal override void OnDrawGraphProperties()
        {
            if (m_clonedGraph == null)
                return;
            int lastDirtyCount = EditorUtility.GetDirtyCount(m_clonedGraph);
            m_adapter.propertiesDrawer.OnDrawProperties(m_clonedGraph);
            if (EditorUtility.GetDirtyCount(m_clonedGraph) > lastDirtyCount)
            {
                UpdateNodesVisual();
                if (m_clonedGraph.HasNode(m_activeNodeId))
                {
                    ExecuteGraph();
                }
            }
        }

        private void UpdateStatusBar()
        {
            string text = $"Texture: {m_currentMemStats.textureCount}/{m_peakMemStats.textureCount} | Buffer: {m_currentMemStats.bufferCount}/{m_peakMemStats.bufferCount} | Memory: {m_currentMemStats.megabyte.ToString("0.0")}/{m_peakMemStats.megabyte.ToString("0.0")} MB";
            m_statusBar.infoRight = text;
        }

        public void Update()
        {
            if (m_lastExecution != null)
            {
                if (m_lastExecution.progress != null)
                {
                    if (m_statusBar.IsProgressVisible())
                    {
                        m_statusBar.SetProgress(m_lastExecution.progress.totalProgress, m_lastExecution.progress.currentProgress);
                    }
                }
            }
        }

        public override void OnGUI()
        {
            if (m_clonedGraph == null)
                return;
            base.OnGUI();
            UpdateStatusBar();
            UpdateToolbarButtonsStatus();

            if (m_viewport2d != null)
            {
                if (m_viewport3d != null && m_viewport3d.resolvedStyle != null)
                {
                    if (m_viewport3d.resolvedStyle.display == DisplayStyle.None)
                    {
                        m_viewport2d.AddToClassList("stretch");
                    }
                    else
                    {
                        m_viewport2d.RemoveFromClassList("stretch");
                    }
                }
            }
            if (m_viewport3d != null)
            {
                if (m_viewport2d != null && m_viewport2d.resolvedStyle != null)
                {
                    if (m_viewport2d.resolvedStyle.display == DisplayStyle.None)
                    {
                        m_viewport3d.AddToClassList("stretch");
                    }
                    else
                    {
                        m_viewport3d.RemoveFromClassList("stretch");
                    }
                }
            }

            if (m_viewport2d != null && m_viewport2d.resolvedStyle != null && m_viewport3d != null && m_viewport3d.resolvedStyle != null)
            {
                bool noViewportDisplayed = m_viewport2d.resolvedStyle.display == DisplayStyle.None && m_viewport3d.resolvedStyle.display == DisplayStyle.None;
                m_viewportContainer.style.display = noViewportDisplayed ? new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }

            if (m_viewport2d != null && m_viewport2d.resolvedStyle != null)
            {
                bool isVisible = m_viewport2d.resolvedStyle.display == DisplayStyle.Flex;
                if (isVisible && m_needConstantUpdate2D)
                {
                    m_viewport2d.MarkDirtyRepaint();
                }
            }

            UpdateTroubleshootingStatus();
        }

        private void UpdateTroubleshootingStatus()
        {
            if (m_clonedGraph == null)
                return;
            EditorSettings editorSettings = EditorSettings.Get();
            bool enableTroubleshooting = editorSettings.troubleshootingSettings.enableTroubleshootingMode;
            if (enableTroubleshooting)
            {
                string[] logs = EditorSettings.TroubleshootingSettings.GetGraphExecLog(m_clonedGraph as TerrainGraph);
                m_graphView.Query<NodeView>().ForEach((nv) =>
                {
                    bool wasRunSuccessfully = ArrayUtility.Contains(logs, $"{nv.nodeId} OK");
                    nv.SetBadgeEnable<CheckmarkBadge>(wasRunSuccessfully);

                    bool wasRunButCrash = ArrayUtility.Contains(logs, nv.nodeId);
                    nv.SetBadgeEnable<XMarkBadge>(wasRunButCrash);
                });
            }
            else
            {
                m_graphView.Query<NodeView>().ForEach((nv) =>
                {
                    nv.SetBadgeEnable<CheckmarkBadge>(false);
                    nv.SetBadgeEnable<XMarkBadge>(false);
                });
            }
        }

        private void OnDebug3dButtonClicked()
        {
            if (m_viewport3d != null)
            {
                m_viewport3d.style.display = new StyleEnum<DisplayStyle>(m_viewport3d.resolvedStyle.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }

        private void OnDebug2dButtonClicked()
        {
            if (m_viewport2d != null)
            {
                m_viewport2d.style.display = new StyleEnum<DisplayStyle>(m_viewport2d.resolvedStyle.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }

        private void UpdateToolbarButtonsStatus()
        {
            bool isViewport3dVisible = false;
            if (m_viewport3d != null && m_viewport3d.resolvedStyle != null)
            {
                isViewport3dVisible = m_viewport3d.resolvedStyle.display == DisplayStyle.Flex;
            }
            m_viewport3dButton.EnableInClassList("active", isViewport3dVisible);

            bool isViewport2dVisible = false;
            if (m_viewport2d != null && m_viewport2d.resolvedStyle != null)
            {
                isViewport2dVisible = m_viewport2d.resolvedStyle.display == DisplayStyle.Flex;
            }
            m_viewport2dButton.EnableInClassList("active", isViewport2dVisible);
        }

        public override void UpdateNodesVisual()
        {
            m_graphView.Query<NodeView>().ForEach(nv =>
            {
                UpdateNodeVisual(nv);
            });
        }

        private void UpdateNodeVisual(NodeView nv)
        {
            INode n = m_clonedGraph.GetNode(nv.nodeId);
            if (n == null)
                return;
            nv.SetBadgeEnable<Display2dBadge>(n.id == m_display2dNodeId);
            nv.SetBadgeEnable<Display3dHeightBadge>(n.id == m_display3dHeightNodeId);
            nv.SetBadgeEnable<Display3dMaskBadge>(n.id == m_display3dMaskNodeId);
            nv.SetBadgeEnable<Display3dPositionsBadge>(n.id == m_display3dPositionNodeId);
            if (n is ExecutableNodeBase enb)
            {
                nv.SetBadgeEnable<SplitExecutionBadge>(enb.shouldSplitExecution && (clonedGraph as TerrainGraph).allowSplitExecution);
                nv.SetBadgeEnable<BypassBadge>(enb.isBypassed);
            }

            if (n is ImageNodeBase inb)
            {
                bool isOverridingResolution = false;
                if (inb.resolutionOverride == ResolutionOverrideOptions.RelativeToGraph ||
                    inb.resolutionOverride == ResolutionOverrideOptions.RelativeToMainInput)
                {
                    if (inb.resolutionMultiplier != 1)
                    {
                        isOverridingResolution = true;
                    }
                }
                else if (inb.resolutionOverride == ResolutionOverrideOptions.Absolute)
                {
                    isOverridingResolution = true;
                }
                nv.SetBadgeEnable<ResolutionOverrideBadge>(isOverridingResolution);
            }
            NodeEditor nodeEditor = NodeEditor.Get(n.GetType());
            if (nodeEditor is INeedUpdateNodeVisual nunv)
            {
                nv.ClearWarnings();
                nunv.UpdateVisual(n, nv);
            }

            updateNodeVisualCallback?.Invoke(this, nv, n);
        }

        private bool IsConnectedToHeightOutputNode(INode node)
        {
            if (node is HeightOutputNode)
            {
                throw new ArgumentException("The node itself is a Height Output node");
            }
            List<IEdge> edges = clonedGraph.GetEdges();
            Stack<INode> trace = new Stack<INode>();
            trace.Push(node);
            while (trace.Count > 0)
            {
                INode n = trace.Pop();
                foreach (IEdge e in edges)
                {
                    if (e.outputSlot.nodeId.Equals(n.id))
                    {
                        INode nextNode = clonedGraph.GetNode(e.inputSlot.nodeId);
                        if (nextNode is HeightOutputNode)
                        {
                            return true;
                        }
                        if (nextNode != null && !trace.Contains(nextNode))
                        {
                            trace.Push(nextNode);
                        }
                    }
                }
            }

            return false;
        }

        protected override void OnDelayCall()
        {
            base.OnDelayCall();
            LoadPrefs();
            UpdateNodesVisual();
        }

        internal override void OnGraphViewBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.OnGraphViewBuildContextualMenu(evt);
            if (evt.target is NodeView nv)
            {
                if (!nv.isAnchorNode)
                {
                    string id = nv.nodeId;
                    INode node = clonedGraph.GetNode(id);

                    evt.menu.AppendSeparator();

                    bool is2d = node.id == m_display2dNodeId;
                    evt.menu.AppendAction("Show as 2D",
                        (a) =>
                        {
                            ToggleDisplayHint(ref m_display2dNodeId, node.id);
                            UpdateNodesVisual();
                        },
                        is2d ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

                    bool is3dHeight = node.id == m_display3dHeightNodeId;
                    evt.menu.AppendAction("Show as Height",
                        (a) =>
                        {
                            ToggleDisplayHint(ref m_display3dHeightNodeId, node.id);
                            UpdateNodesVisual();
                        },
                        is3dHeight ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

                    bool is3dMask = node.id == m_display3dMaskNodeId;
                    evt.menu.AppendAction("Show as Mask",
                        (a) =>
                        {
                            ToggleDisplayHint(ref m_display3dMaskNodeId, node.id);
                            UpdateNodesVisual();
                        },
                        is3dMask ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

                    bool is3dPoints = node.id == m_display3dPositionNodeId;
                    evt.menu.AppendAction("Show as Positions",
                        (a) =>
                        {
                            ToggleDisplayHint(ref m_display3dPositionNodeId, node.id);
                            UpdateNodesVisual();
                        },
                        is3dPoints ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                }
            }
        }

        private void ToggleDisplayHint(ref string targetHint, string nodeId)
        {
            if (targetHint.Equals(nodeId))
            {
                targetHint = string.Empty;
            }
            else
            {
                targetHint = nodeId;
            }
        }

        protected override void ValidateGraph()
        {
            bool hasChanged = clonedGraph.Validate();
            if (hasChanged)
            {
                m_graphView.RemoveAllElementsFromView();
                m_graphView.LoadGraph(clonedGraph);
            }
        }

        internal virtual void OnViewport2dIMGUI(Rect imguiRect, Rect imageRect)
        {
            if (m_clonedGraph == null)
                return;
            INode activeNode = clonedGraph.GetNode(m_activeNodeId);
            if (activeNode != null)
            {
                Type nodeType = activeNode.GetType();
                NodeEditor nodeEditor = NodeEditor.Get(nodeType);
                if (nodeEditor == null)
                {
                    nodeEditor = new GenericNodeEditor();
                }

                nodeEditor.m_graphEditor = this;
                m_needConstantUpdate2D = nodeEditor.needConstantUpdate2D;

                EditorGUI.BeginChangeCheck();
                nodeEditor.OnViewport2dGUI(activeNode, imguiRect, imageRect);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(clonedGraph);
                    ExecuteGraph();
                    UpdateNodesVisual();
                }
            }
        }

    }
}
#endif
