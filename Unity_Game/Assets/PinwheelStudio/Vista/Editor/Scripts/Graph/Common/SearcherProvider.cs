#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Searcher;

namespace Pinwheel.VistaEditor.Graph
{
    public class SearcherProvider : ISearcherProvider
    {
        private NodeSearcherAdapter adapter;
        private List<Type> m_nodeTypes;

        private List<Type> m_subGraphTypes;
        private List<Type> m_subGraphNodeTypes;

        public string currentSearchText { get; set; }

        public SearcherProvider()
        {
            m_nodeTypes = GetNodeTypes();
            adapter = new NodeSearcherAdapter("Add Node");

            m_subGraphTypes = new List<Type>();
            m_subGraphNodeTypes = new List<Type>();
        }

        public void SetSubGraphTypes(Type graphAssetType, Type subGraphNodeType)
        {
            m_subGraphTypes.Add(graphAssetType);
            m_subGraphNodeTypes.Add(subGraphNodeType);
        }

        public Searcher GetSearcher(SearcherFilter filter)
        {
            MenuTree<SearcherItemData> tree = GetMenuTree(filter);
            List<SearcherItem> items = GetSearcherItems(tree);
            SearcherDatabase database = new SearcherDatabase(items.AsReadOnly());
            Searcher s = new Searcher(database, adapter);
            return s;
        }

        public SearcherItemData GetDataFromSelectedItem(SearcherItem item)
        {
            if (item is NodeSearcherItem i)
            {
                return i.data;
            }
            else
            {
                return default;
            }
        }

        private List<Type> GetNodeTypes()
        {
            return TypeCache.GetTypesDerivedFrom<INode>().ToList();
        }

        private MenuTree<SearcherItemData> GetMenuTree(SearcherFilter filter)
        {
            MenuTree<SearcherItemData> tree = new MenuTree<SearcherItemData>("", "Node", default);
            foreach (Type t in m_nodeTypes)
            {
                if (filter.typeFilter != null && filter.typeFilter.Invoke(t) == false)
                {
                    continue;
                }

                NodeMetadataAttribute meta = NodeMetadata.Get(t);
                if (meta != null && !string.IsNullOrEmpty(meta.path))
                {
                    SearcherItemData data = new SearcherItemData();
                    data.nodeType = t;
                    data.meta = meta;
                    if (!filter.inspectSlot)
                    {
                        tree.AddEntry(meta.icon, meta.path, data);
                    }
                    else
                    {
                        InspectSlots(filter, t, meta, tree);
                    }
                }
            }

            for (int i = 0; i < m_subGraphTypes.Count; ++i)
            {
                string[] guid = AssetDatabase.FindAssets($"t:{m_subGraphTypes[i].Name}");
                foreach (string id in guid)
                {
                    string path = AssetDatabase.GUIDToAssetPath(id);
                    if (path.Equals(filter.sourceGraphPath))
                        continue;
                    Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                    if (!m_subGraphTypes[i].Equals(assetType))
                        continue;

                    string graphName = Path.GetFileNameWithoutExtension(path);
                    if (graphName.StartsWith("~"))
                        continue;

                    NodeMetadataAttribute meta = NodeMetadata.Get(m_subGraphNodeTypes[i]);
                    SearcherItemData data = new SearcherItemData();
                    data.nodeType = m_subGraphNodeTypes[i];
                    data.meta = meta;
                    data.hint = path;
                    tree.AddEntry(meta.icon, "Sub Graphs/" + graphName, data);
                }
            }

            return tree;
        }

        private void InspectSlots(SearcherFilter filter, Type nodeType, NodeMetadataAttribute meta, MenuTree<SearcherItemData> tree)
        {
            INode n = Activator.CreateInstance(nodeType) as INode;
            if (n == null)
                return;
            ISlot[] slots;
            if (filter.slotDirection == SlotDirection.Input)
            {
                slots = n.GetInputSlots();
            }
            else if (filter.slotDirection == SlotDirection.Output && !(n is IOutputNode))
            {
                slots = n.GetOutputSlots();
            }
            else
            {
                slots = new ISlot[0];
            }
            if (filter.slotAdapter != null)
            {
                slots = slots.Where(s => filter.slotAdapter.CanConnectTo(s.GetAdapter())).ToArray();
            }
            for (int i = 0; i < slots.Length; ++i)
            {
                ISlot s = slots[i];
                SearcherItemData data = new SearcherItemData();
                data.nodeType = nodeType;
                data.meta = meta;
                data.slotId = s.id;
                data.slotName = s.name;
                data.slotIndex = i;
                tree.AddEntry(meta.icon, meta.path + $" ({s.name})", data);
            }
        }

        private List<SearcherItem> GetSearcherItems(MenuTree<SearcherItemData> tree)
        {
            Stack<MenuEntryGroup<SearcherItemData>> s0 = new Stack<MenuEntryGroup<SearcherItemData>>();
            for (int i = 0; i < tree.RootEntry.subEntries.Count; ++i)
            {
                s0.Push(tree.RootEntry.subEntries[i]);
            }

            List<SearcherItem> items = new List<SearcherItem>();
            Stack<SearcherItem> s1 = new Stack<SearcherItem>();
            for (int i = 0; i < tree.RootEntry.subEntries.Count; ++i)
            {
                MenuEntryGroup<SearcherItemData> entry = tree.RootEntry.subEntries[i];
                string[] synonyms = null;
                string help = null;
                if (entry.data != null)
                {
                    if (entry.data.meta != null && entry.data.meta.keywords != null && SearcherUtils.IsSmartSearchSupported())
                    {
                        synonyms = entry.data.meta.keywords.Split(',');
                    }
                    if (entry.data.meta != null && entry.data.meta.description != null)
                    {
                        help = NodeMetaUtilities.ParseDescription(entry.data.meta.description);
                    }
                }
                NodeSearcherItem childItem = new NodeSearcherItem(entry.text, help);
                childItem.data = entry.data;
                childItem.Synonyms = synonyms;

                items.Add(childItem);
                s1.Push(childItem);
            }

            while (s0.Count > 0)
            {
                MenuEntryGroup<SearcherItemData> treeEntry = s0.Pop();
                SearcherItem item = s1.Pop();

                for (int i = 0; i < treeEntry.subEntries.Count; ++i)
                {
                    MenuEntryGroup<SearcherItemData> entry = treeEntry.subEntries[i];
                    string[] synonyms = null;
                    string help = null;
                    if (entry.data != null)
                    {
                        if (entry.data.meta != null && entry.data.meta.keywords != null && SearcherUtils.IsSmartSearchSupported())
                        {
                            synonyms = entry.data.meta.keywords.Split(',');
                        }
                        if (entry.data.meta != null && entry.data.meta.description != null)
                        {
                            help = NodeMetaUtilities.ParseDescription(entry.data.meta.description);
                        }
                    }

                    NodeSearcherItem childItem = new NodeSearcherItem(entry.text, help);
                    childItem.data = entry.data;
                    childItem.Synonyms = synonyms;

                    item.AddChild(childItem);
                    s0.Push(entry);
                    s1.Push(childItem);
                }
            }

            return items;
        }
    }
}
#endif
