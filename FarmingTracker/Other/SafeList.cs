using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    /// <summary>
    /// Because SynchronizedCollection is not thread safe for foreach and ToList() and ConcurrentBag and ConcurrentDictionary is harder to use as List replacement
    /// </summary>
    public class SafeList<T> : IEnumerable<T>
    {
        public SafeList() : this(new List<T>())
        {
        }

        public SafeList(List<T> list)
        {
            _list = list.ToList(); // ToList() to prevent using the same reference
        }

        public void AddSafe(T item)
        {
            lock (_lock)
                _list.Add(item);
        }

        public void RemoveSafe(T item)
        {
            lock (_lock)
                _list.Remove(item);
        }

        public bool ContainsSafe(T item)
        {
            lock (_lock)
                return _list.Contains(item);
        }

        public void ClearSafe()
        {
            lock (_lock)
                _list.Clear();
        }

        public bool AnySafe()
        {
            lock (_lock)
                return _list.Any();
        }

        public bool AnySafe(Func<T, bool> predicate)
        {
            lock (_lock)
                return _list.Any(predicate);
        }

        public List<T> ToListSafe()
        {
            lock (_lock)
                return _list.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
                return ToListSafe().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly object _lock = new object();
        private readonly List<T> _list;
    }
}
