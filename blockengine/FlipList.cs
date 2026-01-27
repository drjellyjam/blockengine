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
            if (active) { return list1; }
            return list2;
        }

        public List<T> GetInactiveList()
        {
            if (!active) { return list1; }
            return list2;
        }

        public void Flip()
        {
            active = !active;
        }

        public void Add(T item) {
            if (active) { Console.WriteLine("LIST1"); } else { Console.WriteLine("LIST2"); }
            var _list = GetActiveList();
            if (allow_duplicates || !_list.Contains(item))
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            GetActiveList().Clear();
        }
    }
}
