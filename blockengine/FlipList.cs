using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class FlipList<T>
    {
        private List<T> list1;
        private List<T> list2;

        public bool active = true;
        private bool allow_duplicates;

        public FlipList(bool allow_duplicates)
        {
            list1 = new List<T>();
            list2 = new List<T>();
            this.allow_duplicates = allow_duplicates;
        }

        public List<T> GetActiveList()
        {
            if (active) { return list2; }
            return list1;
        }

        public List<T> GetInactiveList()
        {
            if (!active) { return list2; }
            return list1;
        }

        public void Flip()
        {
            active = !active;
        }

        public void Add(T item) {
            //if (active) { Console.WriteLine("LIST1"); } else { Console.WriteLine("LIST2"); }
            var _list = GetActiveList();
            if (allow_duplicates || !_list.Contains(item))
            {
                _list.Add(item);
            }
        }

        public void Remove(T item)
        {
            var _list = GetActiveList();
            _list.Remove(item);
        }
        public bool Has(T item)
        {
            return list1.Contains(item) || list2.Contains(item);
        }

        public int Count()
        {
            return list1.Count + list2.Count;
        }

        public void Clear()
        {
            GetActiveList().Clear();
        }
    }
}
