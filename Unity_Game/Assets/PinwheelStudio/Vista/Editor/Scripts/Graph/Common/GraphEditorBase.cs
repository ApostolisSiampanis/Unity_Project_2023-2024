#if VISTA
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
using Resizer = Pinwheel.VistaEditor.UIElements.Resizer;
using System.IO;

namespace Pinwheel.VistaEditor.Graph
{
    public class GraphEditorBase : EditorWindow
    {
        public delegate void UpdateNodeVisualHandler(GraphEditorBase editor, NodeView nv, INode node);
        public static UpdateNodeVisualHandler updateNodeVisualCallback;

        [SerializeField]
        protected GraphAsset m_sourceGraph;
        public GraphAsset sourceGraph
        {
            get
            {
                return m_sourceGraph;
            }
            internal set
            {
                m_sourceGraph = value;
            }
        }

        [SerializeField]
        protected GraphAsset m_clonedGraph;
        public GraphAsset clonedGraph
        {
            get
            {
                return m_clonedGraph;
            }
        }

        [SerializeField]
        protected bool m_dontDisplaySaveDialogOnExit;
        public bool dontDisplaySaveDialogOnExit
        {
            get
            {
                return m_dontDisplaySaveDialogOnExit;
            }
            set
            {
                m_dontDisplaySaveDialogOnExit = value;
            }
        }

        [SerializeReference]
        protected IExternalInputProvider m_externalInputProvider;
        public IExternalInputProvider externalInputProvider
        {
            get
            {
                return m_externalInputProvider;
            }
        }

        internal GraphEditorTitle m_title;
        internal GraphEditorToolbar m_toolbar;

        internal VisualElement m_bodyContainer;
        internal VisualElement m_leftContainer;
        internal VisualElement m_rightContainer;

        internal VisualElement m_graphViewContainer;
        internal GraphEditorGraphView m_graphView;

        private ToolbarButton m_rightPanelButton;
        internal PropertiesView m_propertiesView;

        internal GraphEditorStatusBar m_statusBar;

        internal string m_activeNodeId;

        private const string PREF_RIGHT_PANEL_VISIBLE = "graph-editor-right-panel-visible";
        private const string PREF_RIGHT_PANEL_WIDTH = "graph-editor-right-panel-width";

        internal static List<GraphEditorBase> openingEditors { get; set; }

        public static T OpenGraph<T>(GraphAsset graph, IExternalInputProvider inputProvider = null) where T : GraphEditorBase
        {
            List<GraphEditorBase> openingEditors = GetOpeningEditors();
            T existingEditor = openingEditors.Find(e => e.sourceGraph == graph && e is T) as T;
            if (existingEditor != null)
            {
                existingEditor.Focus();
                existingEditor.m_externalInputProvider = inputProvider;
                return existingEditor;
            }
            else
            {
                T editor = CreateWindow<T>(typeof(GraphEditorBase), typeof(TerrainGraphEditor), typeof(BiomeMaskGraphEditor), typeof(SceneView));
                editor.m_sourceGraph = graph;
                editor.m_clonedGraph = CloneSourceGraphForEditing(graph);
                editor.m_externalInputProvider = inputProvider;
                editor.Show();
                return editor;
            }
        }

        private static GraphAsset CloneSourceGraphForEditing(GraphAsset graph)
        {
            GraphAsset clone = Instantiate(graph);
            string tempDirectory = EditorSettings.GetTempFolderPath();
            string filePath = Path.Combine(tempDirectory, $"~Temp_{graph.name}.asset");
            AssetDatabase.CreateAsset(clone, filePath);

            return clone;
        }

        public static List<GraphEditorBase> GetOpeningEditors()
        {
            List<GraphEditorBase> editorList = new List<GraphEditorBase>();
            if (openingEditors != null)
            {
                editorList.AddRange(openingEditors);
            }
            return editorList;
        }

        private void AddToEditorList(GraphEditorBase e)
        {
            if (openingEditors == null)
            {
                openingEditors = new List<GraphEditorBase>();
            }
            if (!openingEditors.Contains(e))
            {
                openingEditors.Add(e);
            }
        }

        private void RemoveFromEditorList(GraphEditorBase e)
        {
            if (openingEditors != null)
            {
                openingEditors.RemoveAll(editor => editor == e);
            }
        }

        public virtual void OnEnable()
        {
            AddToEditorList(this);
            SetupGUI();
            RegisterCallbacks();
        }

        public virtual void OnDisable()
        {
            RemoveFromEditorList(this);
            UnregisterCallbacks();
            TearDownGUI();
        }

        private void SetupGUI()
        {
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Vista/USS/Graph/GraphEditor"));

            m_title = new GraphEditorTitle(this);

            m_toolbar = new GraphEditorToolbar(this);
            rootVisualElement.Add(m_toolbar);

            m_bodyContainer = new VisualElement();
            m_bodyContainer.name = "body-container";
            rootVisualElement.Add(m_bodyContainer);

            m_leftContainer = new VisualElement();
            m_leftContainer.name = "left-container";
            m_bodyContainer.Add(m_leftContainer);

            m_rightContainer = new VisualElement();
            m_rightContainer.name = "right-container";
            m_bodyContainer.Add(m_rightContainer);

            bool v = EditorPrefs.GetBool(PREF_RIGHT_PANEL_VISIBLE, true);
            m_rightContainer.style.display = new StyleEnum<DisplayStyle>(v ? DisplayStyle.Flex : DisplayStyle.None);

            float w = EditorPrefs.GetFloat(PREF_RIGHT_PANEL_WIDTH, -1);
            if (w > 0)
            {
                m_rightContainer.style.width = new StyleLength(w);
            }
            else
            {
                m_rightContainer.style.width = new StyleLength(400);
            }

            m_graphViewContainer = new VisualElement() { name = "graph-view-container" }; //graph snapping only work if the graph view is the only child of its parent
            m_leftContainer.Add(m_graphViewContainer);
            m_graphView = new GraphEditorGraphView(this);
            m_graphView.activeNodeChanged += OnActiveNodeChanged;
            m_graphViewContainer.Add(m_graphView);

            m_rightPanelButton = new ToolbarButton() { text = "Properties" };
            m_rightPanelButton.clicked += OnRightPanelButtonClicked;
            m_toolbar.rightContainer.Add(m_rightPanelButton);
            m_toolbar.rightContainer.Add(new VerticalSeparator());

            m_propertiesView = new PropertiesView(this);
            m_rightContainer.Add(m_propertiesView);
            m_propertiesView.OnEnable();

            Resizer propertyViewResizer = new Resizer(Resizer.Position.Left);
            propertyViewResizer.AttachTo(m_rightContainer);

            m_statusBar = new GraphEditorStatusBar(this);
            rootVisualElement.Add(m_statusBar);

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);


            OnSetupGUI();

            EditorCoroutineUtility.StartCoroutine(DelayCallCoroutine(), this);
        }

        private void TearDownGUI()
        {
            OnTearDownGUI();

            if (m_rightContainer.resolvedStyle != null)
            {
                bool v = m_rightContainer.resolvedStyle.display == DisplayStyle.Flex ? true : false;
                EditorPrefs.SetBool(PREF_RIGHT_PANEL_VISIBLE, v);

                float w = m_rightContainer.resolvedStyle.width;
                EditorPrefs.SetFloat(PREF_RIGHT_PANEL_WIDTH, w);
            }

            m_propertiesView.OnDisable();
            rootVisualElement.styleSheets.Clear();
            rootVisualElement.Remove(m_toolbar);
            rootVisualElement.Remove(m_bodyContainer);
            rootVisualElement.Remove(m_statusBar);
        }

        private void RegisterCallbacks()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            //EditorApplication.playModeStateChanged += OnPlayModeStateChaged;
            OnRegisterCallbacks();
        }

        private void UnregisterCallbacks()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            OnUnregisterCallbacks();
        }

        protected virtual void OnActiveNodeChanged(NodeView nv)
        {
            if (nv != null)
            {
                m_activeNodeId = nv.nodeId;
            }
            else
            {
                m_activeNodeId = string.Empty;
            }
        }

        protected virtual void OnSetupGUI() { }
        protected virtual void OnTearDownGUI() { }
        protected virtual void OnRegisterCallbacks() { }
        protected virtual void OnUnregisterCallbacks() { }

        protected virtual void OnUndoRedo()
        {
            m_graphView.OnUndoRedo();
        }

        private IEnumerator DelayCallCoroutine()
        {
            yield return null;
            OnDelayCall();
        }

        protected virtual void OnDelayCall()
        {
            if (m_clonedGraph == null)
            {
                Close();
                return;
            }

            SetupViewDataKeys();
            m_clonedGraph.Validate();
            m_graphView.LoadGraph(m_clonedGraph);
            //EditorUtility.ClearDirty(m_clonedGraph);
        }

        private void SetupViewDataKeys()
        {
            if (sourceGraph != null)
            {
                m_toolbar.viewDataKey = "vista-graph-editor-toolbar" + sourceGraph.name;
                m_graphView.viewDataKey = "vista-graph-editor-graphview" + sourceGraph.name;
            }
        }

        public virtual void OnGUI()
        {
            if (m_title != null)
            {
                m_title.OnGUI();
            }
            if (m_toolbar != null)
            {
                m_toolbar.OnGUI();
            }

            bool isRightPanelVisible = false;
            if (m_rightContainer != null)
            {
                isRightPanelVisible = m_rightContainer.resolvedStyle.display == DisplayStyle.Flex;
            }
            if (m_rightPanelButton != null)
            {
                m_rightPanelButton.EnableInClassList("active", isRightPanelVisible);
            }
        }

        public void OnDestroy()
        {
            Undo.ClearUndo(m_clonedGraph);
            if (!dontDisplaySaveDialogOnExit)
                CheckAndPromptSaveChanges();
            if (AssetDatabase.Contains(m_clonedGraph))
            {
                string path = AssetDatabase.GetAssetPath(m_clonedGraph);
                AssetDatabase.DeleteAsset(path);
            }
            string tempDirectory = EditorSettings.GetTempFolderPath();
            string[] filesInDirectory = Directory.GetFiles(tempDirectory);
            if (filesInDirectory.Length == 0)
            {
                AssetDatabase.DeleteAsset(tempDirectory);
            }
        }

        private void CheckAndPromptSaveChanges()
        {
            bool isSourceGraphPersistent = sourceGraph != null && EditorUtility.IsPersistent(sourceGraph);
            bool isCloneGraphDirty = clonedGraph != null && EditorUtility.IsDirty(clonedGraph);

            bool needSaveToExistingAsset = isSourceGraphPersistent && isCloneGraphDirty;
            bool needSaveToNewAsset = !isSourceGraphPersistent;

            if (needSaveToExistingAsset || needSaveToNewAsset)
            {
                string assetName =
                    needSaveToExistingAsset ? AssetDatabase.GetAssetPath(sourceGraph) :
                    needSaveToNewAsset ? clonedGraph.name : null;
                if (EditorUtility.DisplayDialog(
                    "Graph has been modified",
                    "You have made some changes to the graph \n" +
                    $"{assetName}\n" +
                    $"Do you want to save before closing?",
                    "Save", "Don't Save"))
                {
                    if (needSaveToExistingAsset)
                        OnSaveRequest();
                    else
                        OnSaveAsRequest();
                }
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                OnSaveRequest();
            }
        }

        public void RegisterUndo(string operationName)
        {
            Undo.RegisterCompleteObjectUndo(m_clonedGraph, operationName);
        }

        public void RegisterUndo(INode n)
        {
            Undo.RegisterCompleteObjectUndo(m_clonedGraph, $"Modify {ObjectNames.NicifyVariableName(n.GetType().Name)}");
        }

        internal virtual void OnSaveRequest()
        {

        }

        internal virtual void OnSaveAsRequest()
        {

        }

        internal virtual void OnDrawNodeProperties()
        {
            if (m_clonedGraph == null)
                return;
            INode activeNode = clonedGraph.GetNode(m_activeNodeId);
            if (activeNode != null)
            {
                Type nodeType = activeNode.GetType();
                string displayName = ObjectNames.NicifyVariableName(nodeType.Name);
                NodeMetadataAttribute meta = NodeMetadata.Get(nodeType);
                if (meta != null && !string.IsNullOrEmpty(meta.title))
                {
                    displayName = meta.title;
                }
                EditorGUILayout.LabelField(displayName.ToUpper(), EditorStyles.boldLabel);
                NodeEditor nodeEditor = NodeEditor.Get(nodeType);
                if (nodeEditor == null)
                {
                    nodeEditor = new GenericNodeEditor();
                }

                nodeEditor.m_graphEditor = this;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("BASE PROPERTIES", EditorStyles.boldLabel);
                nodeEditor.OnBaseGUI(activeNode);
                if (nodeEditor.hasSpecificProperties)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SPECIFIC PROPERTIES", EditorStyles.boldLabel);
                    nodeEditor.OnGUI(activeNode);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(clonedGraph);
                    ExecuteGraph();
                    UpdateNodesVisual();
                }

                EditorGUI.BeginChangeCheck();
                nodeEditor.OnExposedPropertiesGUI(activeNode);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(clonedGraph);
                    UpdateNodesVisual();
                }
            }
        }

        internal virtual void OnDrawGraphProperties()
        {

        }

        internal virtual ISearcherProvider GetSearcherProvider()
        {
            return null;
        }

        internal virtual void OnGraphViewBuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        private void OnRightPanelButtonClicked()
        {
            if (m_rightContainer.resolvedStyle.display == DisplayStyle.Flex)
            {
                m_rightContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                m_rightContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
        }

        protected virtual void ValidateGraph() { }

        internal virtual void ExecuteGraph()
        {
        }

        public virtual void UpdateNodesVisual() { }
    }
}
#endif
