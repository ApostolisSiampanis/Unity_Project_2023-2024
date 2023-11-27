#if VISTA
using System.Collections.Generic;

namespace Pinwheel.VistaEditor
{
    public class MenuTree<T>
    {
        public MenuEntryGroup<T> RootEntry { get; set; }

        public MenuTree(string rootIcon, string rootText, T rootData)
        {
            RootEntry = new MenuEntryGroup<T>();
            RootEntry.level = 0;
            RootEntry.icon = rootIcon;
            RootEntry.path = rootText;
            RootEntry.text = rootText;
            RootEntry.data = rootData;
        }

        public void AddEntry(string icon, string menuPath, T data)
        {
            string[] paths = menuPath.Split(new string[] { "/" }, System.StringSplitOptions.RemoveEmptyEntries);
            MenuEntryGroup<T> parentEntry = RootEntry;

            for (int i = 0; i < paths.Length; ++i)
            {
                MenuEntryGroup<T> currentEntry = parentEntry.subEntries.Find(e => string.Compare(paths[i], e.text) == 0);
                if (currentEntry != null)
                {
                    parentEntry = currentEntry;
                }
                else
                {
                    currentEntry = new MenuEntryGroup<T>();
                    currentEntry.level = i + 1;
                    currentEntry.text = paths[i];

                    parentEntry.subEntries.Add(currentEntry);

                    parentEntry = currentEntry;
                }

                if (i == paths.Length - 1)
                {
                    currentEntry.icon = icon;
                    currentEntry.data = data;
                    currentEntry.path = menuPath;
                }
            }
        }

        public List<MenuEntryGroup<T>> ToList()
        {
            List<MenuEntryGroup<T>> list = new List<MenuEntryGroup<T>>();
            Stack<MenuEntryGroup<T>> entryStack = new Stack<MenuEntryGroup<T>>();
            entryStack.Push(RootEntry);
            while (entryStack.Count > 0)
            {
                MenuEntryGroup<T> entry = entryStack.Pop();
                List<MenuEntryGroup<T>> subEntries = entry.subEntries;
                for (int i = subEntries.Count - 1; i >= 0; --i)
                {
                    entryStack.Push(subEntries[i]);
                }
                list.Add(entry);
            }
            return list;
        }
    }

    public class MenuEntryGroup<T>
    {
        public int level { get; internal set; }
        public string icon { get; set; }
        public string text { get; set; }
        public string path { get; set; }
        public T data { get; set; }

        private List<MenuEntryGroup<T>> m_subEntries;
        public List<MenuEntryGroup<T>> subEntries
        {
            get
            {
                if (m_subEntries == null)
                {
                    m_subEntries = new List<MenuEntryGroup<T>>();
                }
                return m_subEntries;
            }
            set
            {
                m_subEntries = value;
            }
        }
    }
}
#endif
