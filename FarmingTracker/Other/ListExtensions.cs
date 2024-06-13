using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public static class ListExtensions
    {
        public static void AddSafe<T>(this List<T> list, T item)
        {
            lock (_lock)
                list.Add(item);
        }

        public static void RemoveSafe<T>(this List<T> list, T item)
        {
            lock (_lock)
                list.Remove(item);
        }

        public static bool ContainsSafe<T>(this List<T> list, T item)
        {
            lock (_lock)
                return list.Contains(item);
        }

        public static void ClearSafe<T>(this List<T> list)
        {
            lock (_lock)
                list.Clear();
        }

        public static bool AnySafe<T>(this List<T> list)
        {
            lock (_lock)
                return list.Any();
        }

        public static bool AnySafe<T>(this List<T> list, Func<T, bool> predicate)
        {
            lock (_lock)
                return list.Any(predicate);
        }

        /// <summary>
        /// to create a shallow copy
        /// </summary>
        public static List<T> ToListSafe<T>(this List<T> list)
        {
            lock (_lock)
                return list.ToList();
        }

        // lock is static so operations on multiple different lists at the same time lock each other up
        private static readonly object _lock = new object();
    }
}
