#if VISTA
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Pinwheel.Vista.Graph;
using UnityEditor;

namespace Pinwheel.VistaEditor.Graph
{
    public class NodeSearcherAdapter : SearcherAdapter
    {
        private VisualElement m_nodePreviewContainer;
        private Label m_nodeAssemblyLabel;

        public NodeSearcherAdapter(string title) : base(title)
        {
        }

        public override bool HasDetailsPanel
        {
            get
            {
                return true;
            }
        }

        public override void InitDetailsPanel(VisualElement detailsPanel)
        {
            base.InitDetailsPanel(detailsPanel);
            VisualElement helpTextContainer = detailsPanel.Q("windowDetailsVisualContainer");
            if (helpTextContainer != null)
            {
                Label helpLabel = helpTextContainer.Q<Label>();
                if (helpLabel != null)
                {
                    helpLabel.style.fontSize = new StyleLength(12);
                }
            }

            m_nodePreviewContainer = new VisualElement() { name = "nodePreviewContainer" };
            detailsPanel.Add(m_nodePreviewContainer);

            m_nodePreviewContainer.style.paddingLeft = new StyleLength(8);
            m_nodePreviewContainer.style.paddingTop = new StyleLength(8);
            m_nodePreviewContainer.style.paddingRight = new StyleLength(8);
            m_nodePreviewContainer.style.paddingBottom = new StyleLength(8);
            m_nodePreviewContainer.style.alignItems = new StyleEnum<Align>(Align.Center);
            m_nodePreviewContainer.style.flexGrow = new StyleFloat(1);

            m_nodeAssemblyLabel = new Label() { name = "nodeAssemblyLabel" };
            detailsPanel.Add(m_nodeAssemblyLabel);
            m_nodeAssemblyLabel.style.marginBottom = new StyleLength(4);
            m_nodeAssemblyLabel.style.fontSize = new StyleLength(10);
            m_nodeAssemblyLabel.style.color = new StyleColor(new Color32(100, 100, 100, 255));
        }

        public override void OnSelectionChanged(IEnumerable<SearcherItem> items)
        {
            base.OnSelectionChanged(items);
            if (m_nodePreviewContainer != null)
            {
                m_nodePreviewContainer.Clear();
            }
            if (m_nodeAssemblyLabel != null)
            {
                m_nodeAssemblyLabel.text = string.Empty;
            }

            SearcherItem item = items.First();
            NodeSearcherItem i = item as NodeSearcherItem;
            if (i == null)
                return;
            if (i.data == null || i.data.nodeType == null)
                return;

            INode n = Activator.CreateInstance(i.data.nodeType) as INode;
            TerrainSubGraphNode subGraphNode = n as TerrainSubGraphNode;
            if (subGraphNode != null)
            {
                string subgraphPath = i.data.hint;
                TerrainGraph subGraph = AssetDatabase.LoadMainAssetAtPath(subgraphPath) as TerrainGraph;
                subGraphNode.graph = subGraph;
                m_nodeAssemblyLabel.text = $"Located at {subgraphPath}";
            }
            else
            {
                m_nodeAssemblyLabel.text = $"Defined in {i.data.nodeType.Assembly.GetName().Name}";
            }

            NodeView nv = NodeView.Create(n, null);
            m_nodePreviewContainer.Add(nv);

            if (subGraphNode != null && subGraphNode.graph != null)
            {
                nv.title = subGraphNode.graph.name;
            }
        }
    }
}
#endif
