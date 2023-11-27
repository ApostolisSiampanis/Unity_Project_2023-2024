#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = Pinwheel.Vista.Graph.Edge;
using Group = Pinwheel.Vista.Graph.Group;
using GroupViewBase = UnityEditor.Experimental.GraphView.Group;
using Object = UnityEngine.Object;
using StickyImage = Pinwheel.Vista.Graph.StickyImage;
using StickyNote = Pinwheel.Vista.Graph.StickyNote;

namespace Pinwheel.VistaEditor.Graph
{
    public class GraphEditorGraphView : GraphView
    {
        public delegate void BuildContextualHandler(GraphEditorGraphView graphView, ContextualMenuPopulateEvent evt);
        public static event BuildContextualHandler buildContextualCallback;

        public delegate void FilterDragObectHandler(GraphEditorGraphView graphView, Object[] objects, ref bool accepted);
        public static event FilterDragObectHandler filterDragObjectCallback;

        public delegate void ObjectDroppedHander(GraphEditorGraphView graphView, Object[] objects, Vector2 localDropPosition);
        public static event ObjectDroppedHander objectDroppedCallback;

        public delegate void AddUtilityButtonHandler(GraphEditorGraphView graphView, List<UtilityButton> buttons);
        public static event AddUtilityButtonHandler addUtilityButtonCallback;

        public delegate void EdgeDoubleClickedHandler(GraphEditorGraphView graphView, EdgeView edgeView, Vector2 mousePosInGraph);
        public static event EdgeDoubleClickedHandler edgeDoubleClickedCallback;

        public delegate void NodeOverlapEdgeHandler(GraphEditorGraphView graphView, NodeView nodeView, EdgeView edgeView);
        public static event NodeOverlapEdgeHandler nodeOverlapEdgeCallback;

        public delegate void NodeDroppedOnEdgeHandler(GraphEditorGraphView graphView, NodeView nodeView, EdgeView edgeView);
        public static event NodeDroppedOnEdgeHandler nodeDroppedOnEdgeCallback;

        internal GraphEditorBase m_editor;
        private Vector2 m_mousePositionInContentView;
        private EdgeConnectorListener m_edgeConnectorListener;

        private List<ObjectReference> m_assetRefs;

        public NodeView activeNode { get; set; }
        public Action<NodeView> activeNodeChanged { get; set; }
        public Action<NodeView> nodeAdded { get; set; }

        private VisualElement m_buttonContainer;
        private UtilityButton m_frameAllButton;
        private UtilityButton m_takeScreenshotButton;

        public struct AddNodeResult
        {
            public NodeView nodeView { get; set; }
            public INode node { get; set; }
        }

        public struct AddEdgeResult
        {
            public EdgeView edgeView { get; set; }
            public IEdge edge { get; set; }
        }

        public struct AddGroupResult
        {
            public GroupView groupView { get; set; }
            public IGroup group { get; set; }
        }

        public struct AddStickyNoteResult
        {
            public StickyNoteView stickyNoteView { get; set; }
            public IStickyNote stickyNote { get; set; }
        }

        public struct AddStickyImageResult
        {
            public StickyImageView stickyImageView { get; set; }
            public IStickyImage stickyImage { get; set; }
        }

        public struct LoadGraphResult
        {
            public List<AddNodeResult> nodes { get; set; }
            public List<AddEdgeResult> edges { get; set; }
            public List<AddGroupResult> groups { get; set; }
            public List<AddStickyNoteResult> stickyNotes { get; set; }
            public List<AddStickyImageResult> stickyImages { get; set; }
        }

        [System.Serializable]
        public struct SerializeData
        {
            [SerializeField]
            public List<Serializer.JsonObject> m_nodes;
            [SerializeField]
            public List<Serializer.JsonObject> m_edges;
            [SerializeField]
            public List<Serializer.JsonObject> m_groups;
            [SerializeField]
            public List<Serializer.JsonObject> m_stickyNotes;
            [SerializeField]
            public List<Serializer.JsonObject> m_stickyImages;


            public static SerializeData FromJson(string json)
            {
                SerializeData data = new SerializeData();
                data.m_nodes = new List<Serializer.JsonObject>();
                data.m_edges = new List<Serializer.JsonObject>();
                data.m_groups = new List<Serializer.JsonObject>();
                data.m_stickyNotes = new List<Serializer.JsonObject>();
                data.m_stickyImages = new List<Serializer.JsonObject>();
                EditorJsonUtility.FromJsonOverwrite(json, data);
                return data;
            }
        }

        protected override bool canCopySelection => selection.Any((ISelectable s) => s is NodeView || s is GroupViewBase || s is Placemat || s is StickyNoteView || s is StickyImageView);

        protected override bool canCutSelection => selection.Any((ISelectable s) => s is NodeView || s is GroupViewBase || s is Placemat || s is StickyNoteView || s is StickyImageView);
               
        public GraphEditorGraphView(GraphEditorBase editor)
        {
            this.m_editor = editor;

            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/GraphView");
            styleSheets.Add(uss);
            AddToClassList("panel");

            SetupZoom(0.2f, 10f, ContentZoomer.DefaultScaleStep, ContentZoomer.DefaultReferenceScale);

            this.AddManipulator(new ContentDragger()); //pan with middle mouse
            this.AddManipulator(new SelectionDragger()); //node repositioning
            this.AddManipulator(new RectangleSelector()); //draw a rectangle selection
            this.AddManipulator(new ClickSelector()); //select by clicking
            this.AddManipulator(new FreehandSelector()); //hold shift & draw to select


            this.nodeCreationRequest = OnNodeCreationRequest;
            this.graphViewChanged = OnGraphViewChanged;
            this.elementsAddedToGroup = OnElementsAddedToGroup;
            this.elementsRemovedFromGroup = OnElementsRemovedFromGroup;
            this.groupTitleChanged = OnGroupTitleChanged;

            this.serializeGraphElements = OnSerializeElements;
            this.unserializeAndPaste = OnUnserializeAndPasteElements;
            this.canPasteSerializedData = OnCanPasteSerializedData;

            this.m_edgeConnectorListener = new EdgeConnectorListener(this);

            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);

            m_buttonContainer = new VisualElement() { name = "utility-button-container" };
            m_buttonContainer.AddToClassList("utility-button-container");
            Add(m_buttonContainer);

            m_takeScreenshotButton = new UtilityButton() { name = "take-screenshot-button" };
            m_takeScreenshotButton.image = Resources.Load<Texture2D>("Vista/Textures/TakeScreenshot");
            m_takeScreenshotButton.tooltip = "Take a screenshot of the current visible area of the graph view";
            m_takeScreenshotButton.clicked += OnTakeScreenshotButtonClicked;
            m_buttonContainer.Add(m_takeScreenshotButton);

            m_frameAllButton = new UtilityButton() { name = "frame-all-button" };
            m_frameAllButton.image = Resources.Load<Texture2D>("Vista/Textures/FrameAll");
            m_frameAllButton.tooltip = "Frame all (A)";
            m_frameAllButton.clicked += () => { FrameAll(); };
            m_buttonContainer.Add(m_frameAllButton);

            if (addUtilityButtonCallback != null)
            {
                List<UtilityButton> additionalButtons = new List<UtilityButton>();
                addUtilityButtonCallback.Invoke(this, additionalButtons);
                foreach (UtilityButton b in additionalButtons)
                {
                    if (b != null)
                    {
                        m_buttonContainer.Add(b);
                    }
                }
            }
        }

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt.eventTypeId == MouseMoveEvent.TypeId())
            {
                MouseMoveEvent mouseEvent = evt as MouseMoveEvent;
                m_mousePositionInContentView = this.ChangeCoordinatesTo(this.contentViewContainer, mouseEvent.localMousePosition);
            }
            if (evt.eventTypeId == MouseUpEvent.TypeId())
            {
                HandleActiveNode();
            }
        }

        private void HandleActiveNode()
        {
            NodeView n = null;
            if (selection != null)
            {
                if (selection.Count == 1)
                {
                    if (selection[0] is NodeView selectedNode)
                    {
                        n = selectedNode;
                    }
                }
            }
            if (n != activeNode)
            {
                activeNode = n;
                if (activeNodeChanged != null)
                {
                    activeNodeChanged.Invoke(activeNode);
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            ISlotAdapter adapter = (startPort as PortView).adapter;
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(p =>
            {
                ISlotAdapter otherAdapter = (p as PortView).adapter;
                if (adapter.CanConnectTo(otherAdapter))
                {
                    compatiblePorts.Add(p);
                }
            });
            return compatiblePorts;
        }

        public void RemoveAllElementsFromView()
        {
            ClearSelection();
            List<GraphElement> allElements = new List<GraphElement>();
            graphElements.ForEach(e =>
            {
                if (e is NodeView || e is EdgeView || e is GroupView || e is StickyNoteView || e is StickyImageView)
                {
                    allElements.Add(e);
                }
            });
            foreach (GraphElement e in allElements)
            {
                RemoveElement(e);
            }
        }

        public LoadGraphResult LoadGraph(GraphAsset graph)
        {
            RemoveAllElementsFromView();

            List<INode> nodes = graph.GetNodes();
            List<IEdge> edges = graph.GetEdges();
            List<IStickyNote> notes = graph.GetStickyNotes();
            List<IStickyImage> images = graph.GetStickyImages();
            List<IGroup> groups = graph.GetGroups();

            LoadGraphResult result = LoadElementsToView(nodes, edges, notes, images, groups);
            return result;
        }

        private LoadGraphResult LoadElementsToView(List<INode> nodes, List<IEdge> edges, List<IStickyNote> notes, List<IStickyImage> images, List<IGroup> groups)
        {
            LoadGraphResult loadResult = new LoadGraphResult();
            loadResult.nodes = new List<AddNodeResult>();
            loadResult.edges = new List<AddEdgeResult>();
            loadResult.groups = new List<AddGroupResult>();
            loadResult.stickyNotes = new List<AddStickyNoteResult>();
            loadResult.stickyImages = new List<AddStickyImageResult>();

            if (groups != null)
            {
                foreach (IGroup g in groups)
                {
                    GroupView gv = new GroupView();
                    gv.title = g.title;
                    gv.groupId = g.id;
                    gv.SetPosition(g.position);
                    this.AddElement(gv);

                    AddGroupResult groupResult = new AddGroupResult();
                    groupResult.group = g;
                    groupResult.groupView = gv;
                    loadResult.groups.Add(groupResult);
                }
            }

            List<GroupView> groupViews = new List<GroupView>();
            graphElements.ForEach(e =>
            {
                if (e is GroupView gv)
                {
                    groupViews.Add(gv);
                }
            });

            if (nodes != null)
            {
                foreach (INode n in nodes)
                {
                    NodeView nv = AddNodeView(n, new Rect(n.visualState.position, Vector2.zero));
                    if (!string.IsNullOrEmpty(n.groupId))
                    {
                        GroupView gv = groupViews.Find(g => n.groupId.Equals(g.groupId));
                        if (gv != null)
                        {
                            gv.AddElement(nv);
                        }
                    }

                    AddNodeResult nodeResult = new AddNodeResult();
                    nodeResult.node = n;
                    nodeResult.nodeView = nv;
                    loadResult.nodes.Add(nodeResult);
                    if (nodeAdded != null)
                    {
                        nodeAdded.Invoke(nv);
                    }
                }
            }

            Dictionary<SlotRef, Port> portMap = new Dictionary<SlotRef, Port>();
            ports.ForEach(p =>
            {
                SlotRef slotRef = (p as PortView).slotRef;
                portMap.Add(slotRef, p);
            });

            if (edges != null)
            {
                foreach (IEdge e in edges)
                {
                    Port outputPort;
                    Port inputPort;
                    if (!portMap.TryGetValue(e.outputSlot, out outputPort))
                        continue;
                    if (!portMap.TryGetValue(e.inputSlot, out inputPort))
                        continue;

                    EdgeView ev = outputPort.ConnectTo<EdgeView>(inputPort);
                    ev.edgeId = e.id;
                    ev.output = outputPort;
                    ev.input = inputPort;
                    this.AddElement(ev);

                    AddEdgeResult edgeResult = new AddEdgeResult();
                    edgeResult.edge = e;
                    edgeResult.edgeView = ev;
                    loadResult.edges.Add(edgeResult);
                }
            }

            if (notes != null)
            {
                foreach (IStickyNote note in notes)
                {
                    StickyNoteView noteView = new StickyNoteView();
                    noteView.title = note.title;
                    noteView.contents = note.contents;
                    noteView.fontSize = (StickyNoteFontSize)note.fontSize;
                    noteView.theme = (StickyNoteTheme)note.theme;
                    noteView.SetPosition(note.position);
                    noteView.noteId = note.id;
                    noteView.RegisterCallback<StickyNoteChangeEvent>(OnStickyNoteChanged);
                    this.AddElement(noteView);

                    if (!string.IsNullOrEmpty(note.groupId))
                    {
                        GroupView gv = groupViews.Find(g => note.groupId.Equals(g.groupId));
                        if (gv != null)
                        {
                            gv.AddElement(noteView);
                        }
                    }

                    AddStickyNoteResult noteResult = new AddStickyNoteResult();
                    noteResult.stickyNote = note;
                    noteResult.stickyNoteView = noteView;
                    loadResult.stickyNotes.Add(noteResult);
                }
            }

            if (images != null)
            {
                foreach (IStickyImage img in images)
                {
                    StickyImageView imgView = AddStickyImageView(img);
                    if (!string.IsNullOrEmpty(img.groupId))
                    {
                        GroupView gv = groupViews.Find(g => img.groupId.Equals(g.groupId));
                        if (gv != null)
                        {
                            gv.AddElement(imgView);
                        }
                    }

                    AddStickyImageResult imgResult = new AddStickyImageResult();
                    imgResult.stickyImage = img;
                    imgResult.stickyImageView = imgView;
                    loadResult.stickyImages.Add(imgResult);
                }
            }

            foreach (AddNodeResult r in loadResult.nodes)
            {
                r.nodeView.expanded = !r.node.visualState.collapsed;
            }

            return loadResult;
        }

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            ISearcherProvider searcherProvider = m_editor.GetSearcherProvider();
            if (searcherProvider == null)
                return;

            Vector2 windowPosition = context.screenMousePosition;
            Vector2 windowSize = new Vector2(500, 400);
            Rect r = new Rect(windowPosition, windowSize);

            SearcherFilter filter = new SearcherFilter();
            filter.typeFilter = m_editor.clonedGraph.AcceptNodeType;
            filter.sourceGraphPath = AssetDatabase.GetAssetPath(m_editor.sourceGraph);

            Port lastStartPort = m_edgeConnectorListener.lastStartPort;
            if (lastStartPort != null)
            {
                filter.inspectSlot = true;
                filter.slotAdapter = (lastStartPort as PortView).adapter;
                if (lastStartPort.direction == Direction.Input)
                {
                    filter.slotDirection = SlotDirection.Output;
                }
                else
                {
                    filter.slotDirection = SlotDirection.Input;
                }
            }

            SearcherWindow.Show(
                m_editor,
                searcherProvider.GetSearcher(filter),
                (item) => { return OnNodeSearcherItemSelected(item, context); },
                (analyticEvt) => { searcherProvider.currentSearchText = analyticEvt.currentSearchFieldText.ToLower(); },
                r);

            SearcherWindow sw = EditorWindow.GetWindow<SearcherWindow>();
            if (sw != null)
            {
                sw.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Vista/USS/Graph/SearcherWindow"));
            }
        }

        private bool OnNodeSearcherItemSelected(SearcherItem item, NodeCreationContext context)
        {
            EditorCoroutineUtility.StartCoroutine(OnNodeSearcherItemSelectedDelayed(item, context), this);
            return true;
        }

        private IEnumerator OnNodeSearcherItemSelectedDelayed(SearcherItem item, NodeCreationContext context)
        {
            yield return null;
            if (item == null)
            {
                m_edgeConnectorListener.ResetState();
                yield break;
            }

            ISearcherProvider searcherProvider = m_editor.GetSearcherProvider();
            if (searcherProvider == null)
                yield break;

            SearcherItemData itemData = searcherProvider.GetDataFromSelectedItem(item);
            if (itemData == null || itemData.nodeType == null)
            {
                m_edgeConnectorListener.ResetState();
                yield break;
            }
            else
            {
                Vector2 mouseWorldPos = context.screenMousePosition - m_editor.position.position; ;
                Vector2 mouseLocalPos = this.contentViewContainer.WorldToLocal(mouseWorldPos);
                Vector2 nodePosition = mouseLocalPos;
                Vector2 nodeSize = new Vector2(0, 0);
                Rect nodeRect = new Rect(nodePosition, nodeSize);
                string hint = !string.IsNullOrEmpty(itemData.hint) ? itemData.hint : searcherProvider.currentSearchText;
                AddNodeResult result = AddNodeOfType(itemData.nodeType, nodeRect, hint);
                if (result.nodeView != null && m_edgeConnectorListener.lastStartPort != null)
                {
                    m_edgeConnectorListener.ConnectToLastStartPort(result.nodeView, itemData.slotId);
                }

                ClearSelection();
                AddToSelection(result.nodeView);
                HandleActiveNode();
                EditorUtility.SetDirty(m_editor.clonedGraph);
                m_edgeConnectorListener.ResetState();
                yield break;
            }
        }

        public AddNodeResult AddNodeOfType(Type nodeType, Rect rect, string hint = null)
        {
            if (nodeType == typeof(AnchorNode))
                throw new System.ArgumentException("Should not add Anchor Node with this function, use AddAnchorNode instead");

            m_editor.RegisterUndo("Add Node");

            INode n = Activator.CreateInstance(nodeType) as INode;
            n.visualState = new VisualState() { position = rect.position };
            if (!string.IsNullOrEmpty(hint) && n is ISetupWithHint swh)
            {
                swh.SetupWithHint(hint);
            }

            m_editor.clonedGraph.AddNode(n);

            NodeView nv = AddNodeView(n, rect);
            AddNodeResult result = new AddNodeResult();
            result.node = n;
            result.nodeView = nv;
            if (nodeAdded != null)
            {
                nodeAdded.Invoke(nv);
            }

            return result;
        }

        public AddNodeResult AddAnchorNode(Type slotType, Rect rect)
        {
            m_editor.RegisterUndo("Add Anchor Node");

            INode n = new AnchorNode(slotType) as INode;
            n.visualState = new VisualState() { position = rect.position };
            m_editor.clonedGraph.AddNode(n);

            NodeView nv = AddNodeView(n, rect);
            AddNodeResult result = new AddNodeResult();
            result.node = n;
            result.nodeView = nv;
            if (nodeAdded != null)
            {
                nodeAdded.Invoke(nv);
            }

            return result;
        }

        private NodeView AddNodeView(INode n, Rect position)
        {
            NodeView nv = NodeView.Create(n, m_edgeConnectorListener);
            nv.onExpandedStateChanged = OnNodeExpandedStateChanged;
            nv.SetPosition(position);
            this.AddElement(nv);

            if (n is IHasDynamicSlotCount hdsc)
            {
                hdsc.slotsChanged += OnNodeSlotsChanged;
            }

            nv.RegisterCallback<GeometryChangedEvent>(OnNodeGeometryChanged);

            return nv;
        }

        private void OnNodeGeometryChanged(GeometryChangedEvent evt)
        {
            if (nodeOverlapEdgeCallback != null)
            {
                bool callbackInvoked = false;
                Rect newRect = evt.newRect;
                edges.ForEach(e =>
                {
                    if (callbackInvoked)
                        return;

                    EdgeView ev = e as EdgeView;
                    if (ev.Overlaps(newRect))
                    {
                        nodeOverlapEdgeCallback.Invoke(this, evt.target as NodeView, ev);
                        callbackInvoked = true;
                    }
                });
            }
        }

        private void OnNodeExpandedStateChanged(NodeView nv)
        {
            string id = nv.nodeId;
            INode node = m_editor.clonedGraph.GetNode(id);
            if (node != null)
            {
                VisualState visualState = node.visualState;
                visualState.collapsed = !nv.expanded;
                node.visualState = visualState;
            }
        }

        private void OnNodeSlotsChanged(INode n)
        {
            m_editor.clonedGraph.Validate();
            RemoveAllElementsFromView();
            LoadGraph(m_editor.clonedGraph);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.movedElements != null && change.movedElements.Count > 0)
            {
                OnElementsMoved(ref change);
            }
            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
            {
                OnElementsToRemove(ref change);
            }
            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                OnEdgesToCreate(ref change);
            }
            EditorUtility.SetDirty(m_editor.clonedGraph);
            return change;
        }

        private void OnElementsMoved(ref GraphViewChange change)
        {
            m_editor.RegisterUndo("Move Elements");
            foreach (GraphElement element in change.movedElements)
            {
                if (element is NodeView nv0)
                {
                    OnNodeViewMoved(nv0, change.moveDelta);
                }
                else if (element is GroupView gv)
                {
                    OnGroupViewMoved(gv, change.moveDelta);
                }
                else if (element is StickyNoteView snv)
                {
                    OnStickyNoteMoved(snv, change.moveDelta);
                }
                else if (element is StickyImageView siv)
                {
                    OnStickyImageMoved(siv, change.moveDelta);
                }
            }
        }

        private void OnNodeViewMoved(NodeView nv, Vector2 delta)
        {
            string nodeId = nv.nodeId;
            INode node = m_editor.clonedGraph.GetNode(nodeId);
            if (node != null)
            {
                VisualState newState = node.visualState;
                newState.position += delta;
                node.visualState = newState;
            }

            if (nodeDroppedOnEdgeCallback != null)
            {
                bool callbackInvoked = false;
                Rect newRect = nv.GetPosition();
                edges.ForEach(e =>
                {
                    if (callbackInvoked)
                        return;

                    EdgeView ev = e as EdgeView;
                    if (ev.Overlaps(newRect))
                    {
                        nodeDroppedOnEdgeCallback.Invoke(this, nv, ev);
                        callbackInvoked = true;
                    }
                });
            }
        }

        private void OnGroupViewMoved(GroupView gv, Vector2 delta)
        {
            string groupId = gv.groupId;
            IGroup group = m_editor.clonedGraph.GetGroup(groupId);
            if (group != null)
            {
                Rect position = group.position;
                position.position += delta;
                group.position = position;
            }

            List<GraphElement> groupElements = FindElementsInGroup(groupId);
            foreach (GraphElement ge in groupElements)
            {
                if (ge is NodeView nv1)
                {
                    OnNodeViewMoved(nv1, delta);
                }
                else if (ge is StickyNoteView snv)
                {
                    OnStickyNoteMoved(snv, delta);
                }
                else if (ge is StickyImageView siv)
                {
                    OnStickyImageMoved(siv, delta);
                }
            }
        }

        private void OnStickyNoteMoved(StickyNoteView snv, Vector2 delta)
        {
            string id = snv.noteId;
            IStickyNote note = m_editor.clonedGraph.GetStickyNote(id);
            if (note != null)
            {
                Rect position = note.position;
                position.position += delta;
                note.position = position;
            }
        }

        private void OnStickyImageMoved(StickyImageView siv, Vector2 delta)
        {
            string id = siv.imageId;
            IStickyImage img = m_editor.clonedGraph.GetStickyImage(id);
            if (img != null)
            {
                Rect position = img.position;
                position.position += delta;
                img.position = position;
            }
        }

        private void OnElementsToRemove(ref GraphViewChange change)
        {
            m_editor.RegisterUndo("Remove Elements");
            List<GraphElement> moreToRemove = new List<GraphElement>();
            foreach (GraphElement element in change.elementsToRemove)
            {
                if (element is EdgeView ev)
                {
                    string id = ev.edgeId;
                    m_editor.clonedGraph.RemoveEdge(id);
                }
                else if (element is NodeView nv)
                {
                    string id = nv.nodeId;
                    GraphAsset.RemovedElements result = m_editor.clonedGraph.RemoveNode(id);
                    if (result.edges != null)
                    {
                        foreach (IEdge e in result.edges)
                        {
                            moreToRemove.Add(FindEdge(e.id));
                        }
                    }
                }
                else if (element is GroupView gv)
                {
                    string id = gv.groupId;
                    m_editor.clonedGraph.RemoveGroup(id);
                }
                else if (element is StickyNoteView snv)
                {
                    string id = snv.noteId;
                    m_editor.clonedGraph.RemoveStickyNote(id);
                }
                else if (element is StickyImageView siv)
                {
                    string id = siv.imageId;
                    m_editor.clonedGraph.RemoveStickyImage(id);
                }
            }
            change.elementsToRemove.AddRange(moreToRemove);
        }

        private void OnEdgesToCreate(ref GraphViewChange change)
        {
            List<Edge> verifiedEdges = new List<Edge>();
            List<EdgeView> verifiedEdgeViews = new List<EdgeView>();
            foreach (EdgeView ev in change.edgesToCreate)
            {
                SlotRef outputRef = (ev.output as PortView).slotRef;
                SlotRef inputRef = (ev.input as PortView).slotRef;

                Edge e = new Edge(outputRef, inputRef);
                if (m_editor.clonedGraph.WillCreateRecursive(e))
                    continue;
                verifiedEdges.Add(e);

                ev.edgeId = e.id;
                verifiedEdgeViews.Add(ev);
            }

            if (verifiedEdges.Count > 0)
            {
                m_editor.RegisterUndo("Add Edge");
                foreach (IEdge e in verifiedEdges)
                {
                    m_editor.clonedGraph.AddEdge(e);
                }
            }

            change.edgesToCreate.Clear();
            change.edgesToCreate.AddRange(verifiedEdgeViews);
        }

        private EdgeView FindEdge(string id)
        {
            EdgeView ev = null;
            edges.ForEach(e =>
            {
                string edgeId = (e as EdgeView).edgeId;
                if (edgeId.Equals(id))
                {
                    ev = e as EdgeView;
                    return;
                }
            });
            return ev;
        }

        private List<GraphElement> FindElementsInGroup(string groupId)
        {
            List<GraphElement> groupElements = new List<GraphElement>();
            graphElements.ForEach(e =>
            {
                if (e is NodeView nv)
                {
                    string nodeId = nv.nodeId;
                    INode n = m_editor.clonedGraph.GetNode(nodeId);
                    if (n != null && n.groupId.Equals(groupId))
                    {
                        groupElements.Add(nv);
                    }
                }
                else if (e is StickyNoteView snv)
                {
                    string noteId = snv.noteId;
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(noteId);
                    if (note != null && note.groupId.Equals(groupId))
                    {
                        groupElements.Add(snv);
                    }
                }
                else if (e is StickyImageView siv)
                {
                    string imgId = siv.imageId;
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(imgId);
                    if (img != null && img.groupId.Equals(groupId))
                    {
                        groupElements.Add(siv);
                    }
                }
            });
            return groupElements;
        }

        public AddGroupResult AddGroup()
        {
            m_editor.RegisterUndo("Add Group");

            Group g = new Group();
            g.title = "New group";
            m_editor.clonedGraph.AddGroup(g);

            GroupView gv = new GroupView();
            gv.title = g.title;
            gv.groupId = g.id;
            gv.SetPosition(g.position);
            this.AddElement(gv);

            EditorUtility.SetDirty(m_editor.clonedGraph);

            AddGroupResult result = new AddGroupResult();
            result.group = g;
            result.groupView = gv;

            return result;
        }

        public AddGroupResult GroupSelection()
        {
            AddGroupResult addResult = AddGroup();
            List<GraphElement> groupableElements = new List<GraphElement>();
            foreach (ISelectable s in selection)
            {
                if (s is NodeView || s is StickyNoteView || s is StickyImageView)
                {
                    groupableElements.Add(s as GraphElement);
                }
            }

            m_editor.RegisterUndo("Group Elements");
            addResult.groupView.AddElements(groupableElements);
            EditorUtility.SetDirty(m_editor.clonedGraph);
            return addResult;
        }

        protected bool CanGroupSelection()
        {
            foreach (ISelectable s in selection)
            {
                if (s is NodeView || s is StickyNoteView || s is StickyImageView)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool CanUngroupSelection()
        {
            List<GroupView> groupViews = new List<GroupView>();
            graphElements.ForEach(e =>
            {
                if (e is GroupView gv)
                {
                    groupViews.Add(gv);
                }
            });

            foreach (ISelectable s in selection)
            {
                GraphElement e = s as GraphElement;
                foreach (GroupView gv in groupViews)
                {
                    if (gv.ContainsElement(e))
                        return true;
                }
            }
            return false;
        }

        public void UngroupSelection()
        {
            List<GroupView> groupViews = new List<GroupView>();
            graphElements.ForEach(e =>
            {
                if (e is GroupView gv)
                {
                    groupViews.Add(gv);
                }
            });

            foreach (ISelectable s in selection)
            {
                GraphElement e = s as GraphElement;
                foreach (GroupView gv in groupViews)
                {
                    if (gv.ContainsElement(e))
                    {
                        gv.RemoveElement(e);
                    }
                }
            }
        }

        private void OnElementsAddedToGroup(GroupViewBase gv, IEnumerable<GraphElement> elements)
        {
            string groupId = (gv as GroupView).groupId;
            IGroup g = m_editor.clonedGraph.GetGroup(groupId);
            if (g != null)
            {
                g.position = gv.GetPosition();
            }

            m_editor.RegisterUndo("Group Elements");
            foreach (GraphElement e in elements)
            {
                if (e is NodeView nv)
                {
                    string nodeId = nv.nodeId;
                    INode node = m_editor.clonedGraph.GetNode(nodeId);
                    if (node != null)
                    {
                        node.groupId = groupId;
                    }
                }
                else if (e is StickyNoteView snv)
                {
                    string noteId = snv.noteId;
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(noteId);
                    if (note != null)
                    {
                        note.groupId = groupId;
                    }
                }
                else if (e is StickyImageView siv)
                {
                    string imgId = siv.imageId;
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(imgId);
                    if (img != null)
                    {
                        img.groupId = groupId;
                    }
                }
            }
            EditorUtility.SetDirty(m_editor.clonedGraph);
        }

        private void OnElementsRemovedFromGroup(GroupViewBase gv, IEnumerable<GraphElement> elements)
        {
            string groupId = (gv as GroupView).groupId;
            IGroup g = m_editor.clonedGraph.GetGroup(groupId);
            if (g != null)
            {
                g.position = gv.GetPosition();
            }

            m_editor.RegisterUndo("Ungroup Elements");
            foreach (GraphElement e in elements)
            {
                if (e is NodeView nv)
                {
                    string nodeId = nv.nodeId;
                    INode node = m_editor.clonedGraph.GetNode(nodeId);
                    if (node != null && node.groupId.Equals(groupId))
                    {
                        node.groupId = string.Empty;
                    }
                }
                else if (e is StickyNoteView snv)
                {
                    string noteId = snv.noteId;
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(noteId);
                    if (note != null && note.groupId.Equals(groupId))
                    {
                        note.groupId = string.Empty;
                    }
                }
                else if (e is StickyImageView siv)
                {
                    string imgId = siv.imageId;
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(imgId);
                    if (img != null && img.groupId.Equals(groupId))
                    {
                        img.groupId = string.Empty;
                    }
                }
            }
            EditorUtility.SetDirty(m_editor.clonedGraph);
        }

        private void OnGroupTitleChanged(GroupViewBase gv, string s)
        {
            string groupId = (gv as GroupView).groupId;
            IGroup g = m_editor.clonedGraph.GetGroup(groupId);
            if (g != null)
            {
                m_editor.RegisterUndo("Edit Group Title");
                g.title = s;
                EditorUtility.SetDirty(m_editor.clonedGraph);
            }
        }

        public AddStickyNoteResult AddStickyNote()
        {
            return AddStickyNote(Vector2.zero);
        }

        public AddStickyNoteResult AddStickyNote(Vector2 position)
        {
            m_editor.RegisterUndo("Add Sticky Note");

            StickyNote note = new StickyNote();
            note.title = "New note";
            note.contents = "";
            note.fontSize = (int)StickyNoteFontSize.Small;
            note.theme = (int)StickyNoteTheme.Classic;
            note.position = new Rect(position, StickyNoteView.defaultSize);
            m_editor.clonedGraph.AddStickyNote(note);

            StickyNoteView noteView = new StickyNoteView();
            noteView.title = note.title;
            noteView.contents = note.contents;
            noteView.fontSize = (StickyNoteFontSize)note.fontSize;
            noteView.theme = (StickyNoteTheme)note.theme;
            noteView.SetPosition(note.position);
            noteView.noteId = note.id;
            noteView.RegisterCallback<StickyNoteChangeEvent>(OnStickyNoteChanged);
            this.AddElement(noteView);

            EditorUtility.SetDirty(m_editor.clonedGraph);

            AddStickyNoteResult result = new AddStickyNoteResult();
            result.stickyNote = note;
            result.stickyNoteView = noteView;

            return result;
        }

        private void OnStickyNoteChanged(StickyNoteChangeEvent evt)
        {
            StickyNoteView snv = (StickyNoteView)evt.target;
            string id = snv.noteId;
            IStickyNote note = m_editor.clonedGraph.GetStickyNote(id);
            if (note != null)
            {
                m_editor.RegisterUndo("Edit Sticky Note");
                note.title = snv.title;
                note.contents = snv.contents;
                note.fontSize = (int)snv.fontSize;
                note.theme = (int)snv.theme;
                note.position = snv.GetPosition();
                EditorUtility.SetDirty(m_editor.clonedGraph);
            }
        }

        private string OnSerializeElements(IEnumerable<GraphElement> elements)
        {
            SerializeData data = new SerializeData();
            data.m_nodes = new List<Serializer.JsonObject>();
            data.m_edges = new List<Serializer.JsonObject>();
            data.m_groups = new List<Serializer.JsonObject>();
            data.m_stickyNotes = new List<Serializer.JsonObject>();
            data.m_stickyImages = new List<Serializer.JsonObject>();

            m_assetRefs = new List<ObjectReference>();

            foreach (GraphElement element in elements)
            {
                if (element is NodeView nv)
                {
                    string id = nv.nodeId;
                    INode node = m_editor.clonedGraph.GetNode(id);
                    if (node != null)
                    {
                        GraphAsset.SerializeAssetReferences(m_assetRefs, node);
                        if (node is ISerializationCallbackReceiver scr)
                        {
                            scr.OnBeforeSerialize();
                        }
                        data.m_nodes.Add(Serializer.Serialize(node));
                    }
                }
                else if (element is EdgeView ev)
                {
                    string id = ev.edgeId;
                    IEdge edge = m_editor.clonedGraph.GetEdge(id);
                    if (edge != null)
                    {
                        data.m_edges.Add(Serializer.Serialize(edge));
                    }
                }
                else if (element is StickyNoteView snv)
                {
                    string id = snv.noteId;
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(id);
                    if (note != null)
                    {
                        data.m_stickyNotes.Add(Serializer.Serialize(note));
                    }
                }
                else if (element is StickyImageView siv)
                {
                    string id = siv.imageId;
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(id);
                    if (img != null)
                    {
                        data.m_stickyImages.Add(Serializer.Serialize(img));
                    }
                }
                else if (element is GroupView gv)
                {
                    string id = gv.groupId;
                    IGroup group = m_editor.clonedGraph.GetGroup(id);
                    if (group != null)
                    {
                        data.m_groups.Add(Serializer.Serialize(group));
                    }
                }
            }

            string json = EditorJsonUtility.ToJson(data);
            return json;
        }

        private void OnUnserializeAndPasteElements(string operationName, string json)
        {
            SerializeData data = SerializeData.FromJson(json);
            List<INode> nodes = Serializer.Deserialize<INode>(data.m_nodes);
            foreach (INode n in nodes)
            {
                GraphAsset.DeserializeAssetReferences(m_assetRefs, n);
                if (n is ISerializationCallbackReceiver scr)
                {
                    scr.OnAfterDeserialize();
                }
            }

            List<IEdge> edges = Serializer.Deserialize<IEdge>(data.m_edges);
            List<IStickyNote> notes = Serializer.Deserialize<IStickyNote>(data.m_stickyNotes);
            List<IStickyImage> images = Serializer.Deserialize<IStickyImage>(data.m_stickyImages);
            List<IGroup> groups = Serializer.Deserialize<IGroup>(data.m_groups);

            if (nodes.Count > 0 || notes.Count > 0 || groups.Count > 0 || images.Count > 0)
            {
                m_editor.RegisterUndo("Paste Elements");
            }

            Vector2 positionSum = Vector2.zero;
            int positionCount = 0;
            foreach (INode n in nodes)
            {
                positionSum += n.visualState.position;
                positionCount += 1;
            }
            foreach (IStickyNote n in notes)
            {
                positionSum += n.position.position;
                positionCount += 1;
            }
            foreach (IStickyImage i in images)
            {
                positionSum += i.position.position;
                positionCount += 1;
            }
            foreach (IGroup g in groups)
            {
                positionSum += g.position.position;
                positionCount += 1;
            }

            Vector2 centerPosition = Vector2.zero;
            if (positionCount != 0)
            {
                centerPosition = positionSum / positionCount;
            }

            Vector2 positionOffset = m_mousePositionInContentView - centerPosition;

            Dictionary<string, string> idMap = new Dictionary<string, string>();
            foreach (IGroup g in groups)
            {
                string currentId = g.id;
                string newId = IHasIdExtension.GenerateId();
                IHasIdExtension.SetId(g, newId);
                idMap.Add(currentId, newId);

                m_editor.clonedGraph.AddGroup(g);
            }

            foreach (INode n in nodes)
            {
                string currentId = n.id;
                string newId = IHasIdExtension.GenerateId();
                IHasIdExtension.SetId(n, newId);
                idMap.Add(currentId, newId);

                if (idMap.ContainsKey(n.groupId))
                {
                    n.groupId = idMap[n.groupId];
                }
                else
                {
                    n.groupId = string.Empty;
                }

                VisualState visualState = n.visualState;
                visualState.position += positionOffset;
                n.visualState = visualState;

                m_editor.clonedGraph.AddNode(n);
            }

            List<IEdge> validEdges = new List<IEdge>();
            foreach (IEdge e in edges)
            {
                string outputNodeId = e.outputSlot.nodeId;
                if (!idMap.ContainsKey(outputNodeId))
                    continue;
                string inputNodeId = e.inputSlot.nodeId;
                if (!idMap.ContainsKey(inputNodeId))
                    continue;

                string newId = IHasIdExtension.GenerateId();
                SlotRef outputRef = new SlotRef(idMap[outputNodeId], e.outputSlot.slotId);
                SlotRef inputRef = new SlotRef(idMap[inputNodeId], e.inputSlot.slotId);
                IHasIdExtension.SetId(e, newId);
                e.outputSlot = outputRef;
                e.inputSlot = inputRef;

                m_editor.clonedGraph.AddEdge(e);
                validEdges.Add(e);
            }
            edges = validEdges;

            foreach (IStickyNote note in notes)
            {
                string newId = IHasIdExtension.GenerateId();
                IHasIdExtension.SetId(note, newId);

                if (idMap.ContainsKey(note.groupId))
                {
                    note.groupId = idMap[note.groupId];
                }
                else
                {
                    note.groupId = string.Empty;
                }

                Rect position = note.position;
                position.position += positionOffset;
                note.position = position;

                m_editor.clonedGraph.AddStickyNote(note);
            }

            foreach (IStickyImage img in images)
            {
                string newId = IHasIdExtension.GenerateId();
                IHasIdExtension.SetId(img, newId);

                if (idMap.ContainsKey(img.groupId))
                {
                    img.groupId = idMap[img.groupId];
                }
                else
                {
                    img.groupId = string.Empty;
                }

                Rect position = img.position;
                position.position += positionOffset;
                img.position = position;

                m_editor.clonedGraph.AddStickyImage(img);
            }

            LoadGraphResult result = LoadElementsToView(nodes, edges, notes, images, groups);
            ClearSelection();
            foreach (AddNodeResult r in result.nodes)
            {
                AddToSelection(r.nodeView);
            }
            foreach (AddEdgeResult r in result.edges)
            {
                AddToSelection(r.edgeView);
            }
            foreach (AddGroupResult r in result.groups)
            {
                AddToSelection(r.groupView);
            }
            foreach (AddStickyNoteResult r in result.stickyNotes)
            {
                AddToSelection(r.stickyNoteView);
            }
            foreach (AddStickyImageResult r in result.stickyImages)
            {
                AddToSelection(r.stickyImageView);
            }
        }

        private bool OnCanPasteSerializedData(string json)
        {
            try
            {
                SerializeData data = SerializeData.FromJson(json);
                bool canPaste = data.m_nodes.Count > 0 || data.m_groups.Count > 0 || data.m_stickyNotes.Count > 0 || data.m_stickyImages.Count > 0;
                return canPaste;
            }
            catch
            {
                return false;
            }
        }

        internal void OnUndoRedo()
        {
            List<GraphElement> viewsToRemove = GetElementsWithNoBindingData();
            DeleteElements(viewsToRemove);

            List<INode> nodesToAdd = new List<INode>();
            List<IEdge> edgesToAdd = new List<IEdge>();
            List<IStickyNote> notesToAdd = new List<IStickyNote>();
            List<IStickyImage> imagesToAdd = new List<IStickyImage>();
            List<IGroup> groupsToAdd = new List<IGroup>();
            GetElementsWithNoView(nodesToAdd, edgesToAdd, notesToAdd, imagesToAdd, groupsToAdd);
            LoadElementsToView(nodesToAdd, edgesToAdd, notesToAdd, imagesToAdd, groupsToAdd);
            RefreshElements();

            activeNode = null;
            HandleActiveNode();
        }

        private List<GraphElement> GetElementsWithNoBindingData()
        {
            List<GraphElement> result = new List<GraphElement>();
            graphElements.ForEach(e =>
            {
                if (e is NodeView nv)
                {
                    INode node = m_editor.clonedGraph.GetNode(nv.nodeId);
                    if (node == null)
                    {
                        result.Add(e);
                    }
                }
                else if (e is EdgeView ev)
                {
                    IEdge edge = m_editor.clonedGraph.GetEdge(ev.edgeId);
                    if (edge == null)
                    {
                        result.Add(e);
                    }
                }
                else if (e is StickyNoteView snv)
                {
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(snv.noteId);
                    if (note == null)
                    {
                        result.Add(e);
                    }
                }
                else if (e is StickyImageView siv)
                {
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(siv.imageId);
                    if (img == null)
                    {
                        result.Add(e);
                    }
                }
                else if (e is GroupView gv)
                {
                    IGroup group = m_editor.clonedGraph.GetGroup(gv.groupId);
                    if (group == null)
                    {
                        result.Add(e);
                    }
                }
            });
            return result;
        }

        private void GetElementsWithNoView(List<INode> nodeResults, List<IEdge> edgeResults, List<IStickyNote> noteResults, List<IStickyImage> imageResults, List<IGroup> groupResults)
        {
            List<string> nodeIds = new List<string>();
            List<string> edgeIds = new List<string>();
            List<string> noteIds = new List<string>();
            List<string> imageIds = new List<string>();
            List<string> groupIds = new List<string>();

            graphElements.ForEach(e =>
            {
                if (e is NodeView nv)
                {
                    nodeIds.Add(nv.nodeId);
                }
                else if (e is EdgeView ev)
                {
                    edgeIds.Add(ev.edgeId);
                }
                else if (e is StickyNoteView snv)
                {
                    noteIds.Add(snv.noteId);
                }
                else if (e is StickyImageView siv)
                {
                    imageIds.Add(siv.imageId);
                }
                else if (e is GroupView gv)
                {
                    groupIds.Add(gv.groupId);
                }
            });

            List<INode> nodeData = m_editor.clonedGraph.GetNodes();
            foreach (INode n in nodeData)
            {
                if (!nodeIds.Contains(n.id))
                {
                    nodeResults.Add(n);
                }
            }

            List<IEdge> edgeData = m_editor.clonedGraph.GetEdges();
            foreach (IEdge e in edgeData)
            {
                if (!edgeIds.Contains(e.id))
                {
                    edgeResults.Add(e);
                }
            }

            List<IStickyNote> noteData = m_editor.clonedGraph.GetStickyNotes();
            foreach (IStickyNote n in noteData)
            {
                if (!noteIds.Contains(n.id))
                {
                    noteResults.Add(n);
                }
            }

            List<IStickyImage> imageData = m_editor.clonedGraph.GetStickyImages();
            foreach (IStickyImage i in imageData)
            {
                if (!imageIds.Contains(i.id))
                {
                    imageResults.Add(i);
                }
            }

            List<IGroup> groupData = m_editor.clonedGraph.GetGroups();
            foreach (IGroup g in groupData)
            {
                if (!groupIds.Contains(g.id))
                {
                    groupResults.Add(g);
                }
            }
        }

        private void RefreshElements()
        {
            graphElements.ForEach(e =>
            {
                if (e is NodeView nv)
                {
                    INode node = m_editor.clonedGraph.GetNode(nv.nodeId);
                    if (node != null)
                    {
                        nv.SetPosition(new Rect(node.visualState.position, Vector2.zero));
                    }
                }
                else if (e is StickyNoteView snv)
                {
                    IStickyNote note = m_editor.clonedGraph.GetStickyNote(snv.noteId);
                    if (note != null)
                    {
                        snv.title = note.title;
                        snv.contents = note.contents;
                        snv.fontSize = (StickyNoteFontSize)note.fontSize;
                        snv.theme = (StickyNoteTheme)note.theme;
                        snv.SetPosition(note.position);
                    }
                }
                else if (e is StickyImageView siv)
                {
                    IStickyImage img = m_editor.clonedGraph.GetStickyImage(siv.imageId);
                    if (img != null)
                    {
                        siv.SetImage(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(img.textureGuid)));
                        siv.SetPosition(img.position);
                    }
                }
                else if (e is GroupView gv)
                {
                    IGroup group = m_editor.clonedGraph.GetGroup(gv.groupId);
                    if (group != null)
                    {
                        gv.title = group.title;
                        gv.SetPosition(group.position);
                    }

                    List<GraphElement> containedElements = new List<GraphElement>(gv.containedElements);
                    gv.RemoveElementsWithoutNotification(containedElements);
                    List<GraphElement> groupElements = FindElementsInGroup(gv.groupId);
                    foreach (GraphElement ge in groupElements)
                    {
                        if (!gv.ContainsElement(ge))
                        {
                            gv.AddElement(ge);
                        }
                    }
                }
            });
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("Add Sticky Note", (a) =>
                {
                    AddStickyNote(m_mousePositionInContentView);
                });
                evt.menu.AppendSeparator();
            }

            if (evt.target is NodeView || evt.target is StickyNoteView || evt.target is StickyImageView)
            {
                bool canGroupSelection = CanGroupSelection();
                evt.menu.AppendAction("Group Selection", (a) =>
                {
                    AddGroupResult result = GroupSelection();
                    result.groupView.FocusTitleTextField();
                },
                canGroupSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                bool canUngroupSelection = CanUngroupSelection();
                evt.menu.AppendAction("Ungroup Selection", (a) =>
                {
                    UngroupSelection();
                },
                canUngroupSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is NodeView)
            {
                NodeView nv = evt.target as NodeView;
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Open Documentation", (a) =>
                {
                    OpenDocumentation(nv);
                },
                DropdownMenuAction.Status.Normal);
            }

            m_editor.OnGraphViewBuildContextualMenu(evt);

            if (buildContextualCallback != null)
            {
                buildContextualCallback.Invoke(this, evt);
            }
        }

        private void OpenDocumentation(NodeView nv)
        {
            Application.OpenURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.xcjxt2i17yup");
            //if (!string.IsNullOrEmpty(nv.docLink))
            //{
            //    Application.OpenURL(nv.docLink);
            //}
        }

        internal void SyncNodePosition(NodeView nv, Rect currentNodePos)
        {
            string id = nv.nodeId;
            INode n = m_editor.clonedGraph.GetNode(id);
            if (n != null)
            {
                VisualState state = n.visualState;
                state.position = currentNodePos.position;
                n.visualState = state;
            }
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            Object[] assets = DragAndDrop.objectReferences;
            bool isAccepted = false;
            if (filterDragObjectCallback != null)
            {
                filterDragObjectCallback.Invoke(this, assets, ref isAccepted);
            }
            if (isAccepted)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            UnityEngine.Object[] assets = DragAndDrop.objectReferences;
            if (objectDroppedCallback != null)
            {
                objectDroppedCallback.Invoke(this, assets, evt.localMousePosition);
            }
        }

        public AddStickyImageResult AddStickyImage(Texture2D img)
        {
            return AddStickyImage(img, Vector2.zero);
        }

        public AddStickyImageResult AddStickyImage(Texture2D img, Vector2 position)
        {
            AddStickyImageResult result = new AddStickyImageResult();

            IStickyImage image = new StickyImage();
            image.position = new Rect(position, StickyImageView.defaultSize);
            image.textureGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(img));
            result.stickyImage = image;
            m_editor.clonedGraph.AddStickyImage(image);

            StickyImageView imageView = AddStickyImageView(image);
            result.stickyImageView = imageView;

            return result;
        }

        private StickyImageView AddStickyImageView(IStickyImage img)
        {
            StickyImageView imageView = new StickyImageView();
            AddElement(imageView);
            imageView.SetPosition(img.position);
            imageView.SetImage(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(img.textureGuid)));
            imageView.imageId = img.id;
            return imageView;
        }

        private void OnTakeScreenshotButtonClicked()
        {
            string folder = "Assets/VistaScreenshots/";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string fileName = $"{m_editor.sourceGraph.name}_GraphView_{DateTime.Now.Ticks}.png";
            string fullPath = Path.Combine(folder, fileName);
            EditorCoroutineUtility.StartCoroutine(DoTakeScreenshot(fullPath), this);
        }

        private IEnumerator DoTakeScreenshot(string fullPath)
        {
            m_buttonContainer.visible = false;
            yield return null;

            Rect r = this.worldBound;
            r.position += m_editor.position.position;

            Texture2D tex = EditorScreenshot.Capture(r.position, (int)r.width, (int)r.height);
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, data);
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();

            yield return null;
            m_buttonContainer.visible = true;
            Texture2D assetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            EditorGUIUtility.PingObject(assetTex);
            Selection.activeObject = assetTex;
            AssetDatabase.OpenAsset(assetTex);
            Debug.Log($"Screenshot saved at {fullPath}", assetTex);
        }

        internal void InvokeEdgeDoubleClickedCallback(EdgeView ev, Vector2 mousePosInGraph)
        {
            if (edgeDoubleClickedCallback != null)
            {
                edgeDoubleClickedCallback.Invoke(this, ev, mousePosInGraph);
            }
        }
    }
}
#endif
