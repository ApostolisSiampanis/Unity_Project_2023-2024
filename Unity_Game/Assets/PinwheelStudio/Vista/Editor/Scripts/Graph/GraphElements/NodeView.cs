#if VISTA
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class NodeView : Node
    {
        public Action<NodeView> onExpandedStateChanged;

        protected VisualElement m_highlighter { get; set; }
        protected VisualElement m_badgeContainer { get; set; }
        protected VisualElement m_shadow { get; set; }
        protected Image m_preview { get; set; }

        public string nodeId { get; private set; }
        public string docLink { get; private set; }
        public bool isAnchorNode { get; private set; }

        protected List<string> m_warnings;

        internal static readonly Vector2 ANCHOR_NODE_SIZE = new Vector2(60, 38);

        public static NodeView Create(INode node, IEdgeConnectorListener edgeConnectorListener)
        {
            Type nodeType = node.GetType();
            NodeMetadataAttribute meta = NodeMetadata.Get(nodeType);

            NodeView nv = new NodeView();
            nv.nodeId = node.id;

            bool isOutputNode = node is IOutputNode;

            if (meta != null && !string.IsNullOrEmpty(meta.title))
            {
                nv.title = isOutputNode ? meta.title.ToUpper() : meta.title;
            }
            else
            {
                string title = ObjectNames.NicifyVariableName(node.GetType().Name);
                nv.title = isOutputNode ? title.ToUpper() : title;
            }

            if (meta != null && !string.IsNullOrEmpty(meta.description))
            {
                nv.tooltip = NodeMetaUtilities.ParseDescription(meta.description);
            }

            if (meta != null)
            {
                nv.docLink = meta.documentation;
            }            

            ISlot[] inputSlot = node.GetInputSlots();
            foreach (ISlot slot in inputSlot)
            {
                SlotRef slotRef = new SlotRef(node.id, slot.id);
                PortView p = PortView.Create<EdgeView>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, slot.GetType(), edgeConnectorListener);
                p.portName = slot.name;
                p.slotRef = slotRef;
                p.adapter = slot.GetAdapter();
                nv.inputContainer.Add(p);
            }
            if (inputSlot.Length == 0)
            {
                nv.inputContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            ISlot[] outputSlot = node.GetOutputSlots();
            foreach (ISlot slot in outputSlot)
            {
                SlotRef slotRef = new SlotRef(node.id, slot.id);
                PortView p = PortView.Create<EdgeView>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, slot.GetType(), edgeConnectorListener);
                p.portName = slot.name;
                p.slotRef = slotRef;
                p.adapter = slot.GetAdapter();
                nv.outputContainer.Add(p);
            }
            if (outputSlot.Length == 0 || node is IOutputNode)
            {
                nv.outputContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            nv.RefreshExpandedState();                       

            nv.isAnchorNode = node is AnchorNode;
            nv.DoInlineStyling(meta);

            return nv;
        }

        public NodeView() : base()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/NodeView");
            styleSheets.Add(uss);

            m_shadow = new VisualElement() { name = "shadow" };
            m_shadow.pickingMode = PickingMode.Ignore;
            this.Add(m_shadow);
            m_shadow.SendToBack();

            m_highlighter = new VisualElement() { name = "highlighter" };
            m_highlighter.pickingMode = PickingMode.Ignore;
            this.Add(m_highlighter);
            SetHighlight(false);

            m_badgeContainer = new VisualElement() { name = "badge-container" };
            this.Add(m_badgeContainer);
        }

        private void DoInlineStyling(NodeMetadataAttribute meta)
        {
            UQueryBuilder<VisualElement> query = this.Query<VisualElement>();
            UQueryState<VisualElement> dividers = query.Name("divider").Build();
            dividers.ForEach(d =>
            {
                d.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            });

            string category = null;
            if (meta != null)
            {
                category = meta.GetCategory();
            }

            NodeViewColor colors = GetColors(category);

            this.inputContainer.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
            this.outputContainer.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));

            this.titleContainer.style.backgroundColor = new StyleColor(colors.mainColor);
            this.titleContainer.style.height = new StyleLength(24);
            this.titleContainer.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

            this.topContainer.style.backgroundColor = new StyleColor(colors.secondaryColor);
            this.mainContainer.style.borderLeftWidth = new StyleFloat(0f);
            this.mainContainer.style.borderTopWidth = new StyleFloat(0f);
            this.mainContainer.style.borderRightWidth = new StyleFloat(0f);
            this.mainContainer.style.borderBottomWidth = new StyleFloat(0f);

            if (isAnchorNode)
            {
                this.titleContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                this.m_badgeContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                this.style.width = new StyleLength(ANCHOR_NODE_SIZE.x);
                this.style.minWidth = new StyleLength(ANCHOR_NODE_SIZE.x);
                this.style.height = new StyleLength(ANCHOR_NODE_SIZE.y);
                this.style.minHeight = new StyleLength(ANCHOR_NODE_SIZE.y);
                this.Query<PortView>().ForEach(pv =>
                {
                    Label portLabel = pv.Q<Label>();
                    if (portLabel != null)
                    {
                        portLabel.style.width = new StyleLength(0f);
                        portLabel.style.marginLeft = new StyleLength(0f);
                        portLabel.style.marginTop = new StyleLength(0f);
                        portLabel.style.marginRight = new StyleLength(0f);
                        portLabel.style.marginBottom = new StyleLength(0f);
                    }
                });
            }
        }

        private NodeViewColor GetColors(string category)
        {
            Color32 main = new Color32(35, 35, 35, 255);
            Color32 secondary = new Color32(55, 55, 55, 255);
            if (string.IsNullOrEmpty(category))
            {
                main = new Color32(35, 35, 35, 255);
            }
            else if (category.Equals("General"))
            {
                main = new Color32(100, 80, 60, 255);
            }
            else if (category.Equals("Adjustments"))
            {
                main = new Color32(100, 80, 60, 255);
            }
            else if (category.Equals("Base Shape"))
            {
                main = new Color32(40, 90, 60, 255);
            }
            else if (category.Equals("Masking"))
            {
                main = new Color32(80, 80, 80, 255);
            }
            else if (category.Equals("Nature"))
            {
                main = new Color32(40, 80, 100, 255);
            }
            else if (category.Equals("IO"))
            {
                main = new Color32(100, 45, 45, 255);
            }
            else if (category.Equals("Data"))
            {
                main = new Color32(80, 50, 100, 255);
            }
            else if (category.Equals("Sub Graphs"))
            {
                main = new Color32(35, 35, 35, 255);
            }
            return new NodeViewColor(main, secondary);
        }

        protected override void ToggleCollapse()
        {
            base.ToggleCollapse();
            if (onExpandedStateChanged != null)
            {
                onExpandedStateChanged.Invoke(this);
            }
        }

        public void SetHighlight(bool highlight)
        {
            if (m_highlighter != null)
            {
                m_highlighter.visible = highlight;
            }
        }

        public T SetBadgeEnable<T>(bool enable) where T : Badge, new()
        {
            T b = m_badgeContainer.Q<T>();
            if (b == null && enable)
            {
                b = new T();
                m_badgeContainer.Add(b);
            }
            if (b != null && !enable)
            {
                m_badgeContainer.Remove(b);
            }
            return b;
        }

        public void SetPreviewImage(Texture img)
        {
            if (img != null)
            {
                if (m_preview == null)
                {
                    m_preview = new Image();
                    //m_preview.style.maxWidth = new StyleLength(100);
                    m_preview.style.maxHeight = new StyleLength(200);
                    m_preview.scaleMode = ScaleMode.ScaleAndCrop;
                    extensionContainer.Add(m_preview);
                    extensionContainer.visible = true;
                }
                m_preview.image = img;
            }
            else
            {
                if (m_preview != null)
                {
                    extensionContainer.Remove(m_preview);
                    extensionContainer.visible = false;
                    m_preview = null;
                }
            }
            RefreshExpandedState();
        }

        public void AddWarning(string w)
        {
            AddWarnings(new string[] { w });
        }

        public void AddWarnings(IEnumerable<string> warnings)
        {
            if (m_warnings == null)
            {
                m_warnings = new List<string>();
            }
            m_warnings.AddRange(warnings);
            m_warnings = m_warnings.Distinct().ToList();

            if (m_warnings.Count > 0)
            {
                WarningBadge warningBadge = SetBadgeEnable<WarningBadge>(true);
                StringBuilder sb = new StringBuilder();
                foreach (string w in m_warnings)
                {
                    sb.AppendLine(w);
                }
                warningBadge.tooltip = sb.ToString().Trim('\n');
            }
            else
            {
                SetBadgeEnable<WarningBadge>(false);
            }
        }

        public void ClearWarnings()
        {
            m_warnings = null;
            SetBadgeEnable<WarningBadge>(false);
        }
    }
}
#endif
