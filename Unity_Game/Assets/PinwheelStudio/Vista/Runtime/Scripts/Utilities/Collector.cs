#if VISTA
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pinwheel.Vista
{
    public class Collector<T> : IEnumerable<T>
    {
        private List<T> m_list;

        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        public Collector()
        {
            m_list = new List<T>();
        }

        public void Add(T item)
        {
            m_list.Add(item);
        }

        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }

        public int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }

        public T At(int index)
        {
            return m_list[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        public List<T> ToList()
        {
            return m_list.Distinct().ToList();
        }

        public T[] ToArray()
        {
            return m_list.Distinct().ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        public void Clear()
        {
            m_list.Clear();
        }
    }
}
#endif
